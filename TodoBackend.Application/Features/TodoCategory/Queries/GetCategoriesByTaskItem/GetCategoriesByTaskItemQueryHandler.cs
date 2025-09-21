using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByTaskItem;

public class GetCategoriesByTaskItemQueryHandler : IRequestHandler<GetCategoriesByTaskItemQuery, Result<IReadOnlyList<CategorySummaryViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetCategoriesByTaskItemQueryHandler> _logger;

    public GetCategoriesByTaskItemQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetCategoriesByTaskItemQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CategorySummaryViewModel>>> Handle(GetCategoriesByTaskItemQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category retrieval for task item ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            // Check if task item exists
            _logger.LogDebug("Checking if task item with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("Task item with ID {TaskItemId} not found after {Duration}ms", 
                    request.TaskItemId, stopwatch.ElapsedMilliseconds);
                return Result<IReadOnlyList<CategorySummaryViewModel>>.Failure("Task item not found");
            }

            var taskItemTitle = taskItem.Title; // Store for logging
            _logger.LogDebug("Found task item ID {TaskItemId} ({TaskItemTitle}), fetching associated categories", 
                request.TaskItemId, taskItemTitle);

            // Get TaskItemCategory relations for this task item
            var taskItemCategories = await _uow.TaskItemCategoryRepository.GetCategoriesByTaskItemIdAsync(request.TaskItemId, cancellationToken);

            _logger.LogDebug("Retrieved {CategoryCount} category relations for task item ID {TaskItemId}", 
                taskItemCategories.Count, request.TaskItemId);

            // Map to CategorySummaryViewModels
            var categorySummaryViewModels = taskItemCategories.Select(tic => new CategorySummaryViewModel
            {
                Id = tic.Category.Id,
                Name = tic.Category.Name
            }).ToList();

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {CategoryCount} categories for task item ID {TaskItemId} ({TaskItemTitle}) in {Duration}ms", 
                categorySummaryViewModels.Count, request.TaskItemId, taskItemTitle, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for relational queries
            if (stopwatch.ElapsedMilliseconds > 300)
            {
                _logger.LogWarning("Slow query detected: GetCategoriesByTaskItem for task ID {TaskItemId} took {Duration}ms (threshold: 300ms)", 
                    request.TaskItemId, stopwatch.ElapsedMilliseconds);
            }

            return Result<IReadOnlyList<CategorySummaryViewModel>>.Success(
                categorySummaryViewModels,
                $"Retrieved {categorySummaryViewModels.Count} categories for task item '{taskItemTitle}' successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve categories for task item ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<CategorySummaryViewModel>>.Failure($"Failed to retrieve categories by task item: {ex.Message}");
        }
    }
}