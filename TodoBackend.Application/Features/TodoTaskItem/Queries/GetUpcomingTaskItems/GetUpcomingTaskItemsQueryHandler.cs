using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetUpcomingTaskItems;

public class GetUpcomingTaskItemsQueryHandler : IRequestHandler<GetUpcomingTaskItemsQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetUpcomingTaskItemsQueryHandler> _logger;

    public GetUpcomingTaskItemsQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetUpcomingTaskItemsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetUpcomingTaskItemsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting upcoming tasks retrieval for user ID {UserId} for next {Days} days", 
            request.UserId, request.Days);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Upcoming tasks retrieval failed - user with ID {UserId} not found", request.UserId);
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            var endDate = DateTime.UtcNow.AddDays(request.Days);
            
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), fetching upcoming tasks for date range: now to {EndDate}", 
                request.UserId, userEmail, endDate.ToString("yyyy-MM-dd"));

            // Get upcoming tasks using repository method
            var upcomingTaskItems = await _uow.TaskItemRepository.GetUpcomingTasksAsync(
                userId: request.UserId,
                days: request.Days,
                ct: cancellationToken);

            _logger.LogDebug("Retrieved {UpcomingTaskCount} upcoming tasks from repository for user ID {UserId}", 
                upcomingTaskItems.Count, request.UserId);

            // Map to ViewModels
            var taskItemViewModels = upcomingTaskItems.Select(task => new TaskItemViewModel
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

            // Calculate priority distribution for planning insights
            var highPriorityCount = taskItemViewModels.Count(t => t.Priority == TodoBackend.Domain.Enums.Priority.High);
            var mediumPriorityCount = taskItemViewModels.Count(t => t.Priority == TodoBackend.Domain.Enums.Priority.Medium);
            var lowPriorityCount = taskItemViewModels.Count(t => t.Priority == TodoBackend.Domain.Enums.Priority.Low);

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {UpcomingTaskCount} upcoming tasks for user ID {UserId} ({UserEmail}) for next {Days} days - Priority distribution: {HighCount} high, {MediumCount} medium, {LowCount} low - in {Duration}ms", 
                taskItemViewModels.Count, request.UserId, userEmail, request.Days, highPriorityCount, mediumPriorityCount, lowPriorityCount, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for upcoming task queries
            if (stopwatch.ElapsedMilliseconds > 800)
            {
                _logger.LogWarning("Slow upcoming tasks query detected: {Duration}ms for user ID {UserId} with {Days} days range (threshold: 800ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId, request.Days);
            }

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} upcoming task items for the next {request.Days} days successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve upcoming task items for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve upcoming task items: {ex.Message}");
        }
    }
}