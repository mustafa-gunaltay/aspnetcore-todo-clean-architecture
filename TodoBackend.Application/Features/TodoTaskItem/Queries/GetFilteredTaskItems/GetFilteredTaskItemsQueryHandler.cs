using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetFilteredTaskItems;

public class GetFilteredTaskItemsQueryHandler : IRequestHandler<GetFilteredTaskItemsQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetFilteredTaskItemsQueryHandler> _logger;

    public GetFilteredTaskItemsQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetFilteredTaskItemsQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetFilteredTaskItemsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting filtered task retrieval for user ID {UserId} with filters: IsCompleted={IsCompleted}, Priority={Priority}, CategoryId={CategoryId}, DateRange={StartDate}-{EndDate}", 
            request.UserId, request.IsCompleted, request.Priority, request.CategoryId, request.StartDate, request.EndDate);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Filtered task retrieval failed - user with ID {UserId} not found", request.UserId);
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            var filterDetails = new List<string>();
            
            // Build filter description for logging
            if (request.IsCompleted.HasValue)
                filterDetails.Add($"IsCompleted={request.IsCompleted.Value}");
            if (request.Priority.HasValue)
                filterDetails.Add($"Priority={request.Priority.Value}");
            if (request.CategoryId.HasValue)
                filterDetails.Add($"CategoryId={request.CategoryId.Value}");
            if (request.StartDate.HasValue)
                filterDetails.Add($"StartDate={request.StartDate.Value:yyyy-MM-dd}");
            if (request.EndDate.HasValue)
                filterDetails.Add($"EndDate={request.EndDate.Value:yyyy-MM-dd}");

            var filterDescription = filterDetails.Count > 0 ? string.Join(", ", filterDetails) : "No filters";
            
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), applying filters: {FilterDescription}", 
                request.UserId, userEmail, filterDescription);

            // Gereksinim 2a & 2b: ITaskItemRepository.GetFilteredAsync() kullan?m?
            var taskItems = await _uow.TaskItemRepository.GetFilteredAsync(
                userId: request.UserId,
                isCompleted: request.IsCompleted,
                priority: request.Priority,
                startDate: request.StartDate,
                endDate: request.EndDate,
                categoryId: request.CategoryId,
                ct: cancellationToken);

            _logger.LogDebug("Retrieved {TaskCount} filtered tasks from repository for user ID {UserId}", 
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

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {TaskCount} filtered tasks for user ID {UserId} ({UserEmail}) with filters [{FilterDescription}] in {Duration}ms", 
                taskItemViewModels.Count, request.UserId, userEmail, filterDescription, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for filtered queries
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow filtered query detected: {Duration}ms for user ID {UserId} with {FilterCount} filters (threshold: 1000ms). Consider optimizing query filters.", 
                    stopwatch.ElapsedMilliseconds, request.UserId, filterDetails.Count);
            }

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} filtered task items successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve filtered task items for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve filtered task items: {ex.Message}");
        }
    }
}