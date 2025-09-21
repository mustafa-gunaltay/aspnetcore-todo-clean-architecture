using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetOverdueTaskItems;

public class GetOverdueTaskItemsQueryHandler : IRequestHandler<GetOverdueTaskItemsQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetOverdueTaskItemsQueryHandler> _logger;

    public GetOverdueTaskItemsQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetOverdueTaskItemsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetOverdueTaskItemsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting overdue tasks retrieval for user ID {UserId}", request.UserId);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Overdue tasks retrieval failed - user with ID {UserId} not found", request.UserId);
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), fetching overdue tasks", 
                request.UserId, userEmail);

            // Get overdue tasks using repository method
            var overdueTaskItems = await _uow.TaskItemRepository.GetOverdueTasksAsync(
                userId: request.UserId,
                ct: cancellationToken);

            _logger.LogDebug("Retrieved {OverdueTaskCount} overdue tasks from repository for user ID {UserId}", 
                overdueTaskItems.Count, request.UserId);

            // Map to ViewModels
            var taskItemViewModels = overdueTaskItems.Select(task => new TaskItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId,
                UserEmail = task.User?.Email,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Categories = task.TaskItemCategories
                    ?.Where(tc => !tc.IsDeleted)
                    .Select(tc => new CategorySummaryViewModel
                    {
                        Id = tc.CategoryId,
                        Name = tc.Category?.Name ?? "Unknown"
                    }).ToList() ?? new List<CategorySummaryViewModel>()
            }).ToList();

            stopwatch.Stop();
            
            // Log with urgency context for overdue tasks
            if (taskItemViewModels.Count > 0)
            {
                _logger.LogInformation("Found {OverdueTaskCount} overdue tasks for user ID {UserId} ({UserEmail}) in {Duration}ms - immediate attention required", 
                    taskItemViewModels.Count, request.UserId, userEmail, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogInformation("No overdue tasks found for user ID {UserId} ({UserEmail}) in {Duration}ms", 
                    request.UserId, userEmail, stopwatch.ElapsedMilliseconds);
            }

            // Performance monitoring for overdue queries
            if (stopwatch.ElapsedMilliseconds > 800)
            {
                _logger.LogWarning("Slow overdue tasks query detected: {Duration}ms for user ID {UserId} (threshold: 800ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId);
            }

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} overdue task items successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve overdue task items for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve overdue task items: {ex.Message}");
        }
    }
}