using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByUserId;

public class GetTaskItemsByUserIdQueryHandler : IRequestHandler<GetTaskItemsByUserIdQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetTaskItemsByUserIdQueryHandler> _logger;

    public GetTaskItemsByUserIdQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetTaskItemsByUserIdQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetTaskItemsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task retrieval for user ID {UserId}", request.UserId);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Task retrieval failed - user with ID {UserId} not found", request.UserId);
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), fetching all user tasks", 
                request.UserId, userEmail);

            // Get tasks by user id using repository method
            var taskItems = await _uow.TaskItemRepository.GetTasksByUserIdAsync(
                userId: request.UserId,
                ct: cancellationToken);

            _logger.LogDebug("Retrieved {TaskCount} tasks from repository for user ID {UserId}", 
                taskItems.Count, request.UserId);

            // Map to ViewModels
            var taskItemViewModels = taskItems.Select(task => new TaskItemViewModel
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

            // Calculate task statistics for logging
            var completedCount = taskItemViewModels.Count(t => t.IsCompleted);
            var pendingCount = taskItemViewModels.Count(t => !t.IsCompleted);

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {TaskCount} tasks for user ID {UserId} ({UserEmail}) - {CompletedCount} completed, {PendingCount} waiting to be completed - in {Duration}ms", 
                taskItemViewModels.Count, request.UserId, userEmail, completedCount, pendingCount, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for user task queries
            if (stopwatch.ElapsedMilliseconds > 800)
            {
                _logger.LogWarning("Slow user task query detected: {Duration}ms for user ID {UserId} with {TaskCount} tasks (threshold: 800ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId, taskItemViewModels.Count);
            }

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} task items for user successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve task items for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve task items by user id: {ex.Message}");
        }
    }
}