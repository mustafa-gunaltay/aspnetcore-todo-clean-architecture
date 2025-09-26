using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByCategory;

public class GetTaskItemsByCategoryQueryHandler : IRequestHandler<GetTaskItemsByCategoryQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetTaskItemsByCategoryQueryHandler> _logger;

    public GetTaskItemsByCategoryQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetTaskItemsByCategoryQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetTaskItemsByCategoryQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task retrieval for category ID {CategoryId}", request.CategoryId);
        
        try
        {
            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Task retrieval failed - category with ID {CategoryId} not found", request.CategoryId);
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("Category not found");
            }

            var categoryName = category.Name; // Store for logging
            _logger.LogDebug("Found category ID {CategoryId} ({CategoryName}), fetching associated tasks", 
                request.CategoryId, categoryName);

            // Get TaskItemCategory relations for this category
            // Repository includes: TaskItem -> User (but NOT TaskItem.TaskItemCategories)
            var taskItemCategories = await _uow.TaskItemCategoryRepository.GetTaskItemsByCategoryIdAsync(request.CategoryId, cancellationToken);

            _logger.LogDebug("Retrieved {TaskRelationCount} task-category relations for category ID {CategoryId}", 
                taskItemCategories.Count, request.CategoryId);

            // Extract unique TaskItem IDs to fetch their categories separately
            var taskItemIds = taskItemCategories.Select(tic => tic.TaskItem.Id).Distinct().ToList();
            
            // Create a dictionary to store categories for each TaskItem
            var taskItemCategoriesMap = new Dictionary<int, List<CategorySummaryViewModel>>();
            
            // Fetch categories for each TaskItem separately to avoid circular reference issues
            if (taskItemIds.Any())
            {
                foreach (var taskItemId in taskItemIds)
                {
                    var categoriesForTask = await _uow.TaskItemCategoryRepository.GetCategoriesByTaskItemIdAsync(taskItemId, cancellationToken);
                    
                    taskItemCategoriesMap[taskItemId] = categoriesForTask
                        .Where(tc => !tc.IsDeleted)
                        .Select(tc => new CategorySummaryViewModel
                        {
                            Id = tc.Category.Id,
                            Name = tc.Category.Name // This works because GetCategoriesByTaskItemIdAsync includes Category
                        })
                        .ToList();
                }
            }

            // Map to TaskItemViewModels using the repository's include structure
            var taskItemViewModels = taskItemCategories.Select(tic => new TaskItemViewModel
            {
                Id = tic.TaskItem.Id,
                Title = tic.TaskItem.Title,
                Description = tic.TaskItem.Description,
                Priority = tic.TaskItem.Priority,
                DueDate = tic.TaskItem.DueDate,
                CompletedAt = tic.TaskItem.CompletedAt,
                IsCompleted = tic.TaskItem.IsCompleted,
                UserId = tic.TaskItem.UserId,
                UserEmail = tic.TaskItem.User?.Email, // ? This works because repository includes TaskItem.User
                CreatedAt = tic.TaskItem.CreatedAt,
                UpdatedAt = tic.TaskItem.UpdatedAt,
                Categories = taskItemCategoriesMap.GetValueOrDefault(tic.TaskItem.Id, new List<CategorySummaryViewModel>())
            }).ToList();

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {TaskCount} tasks for category ID {CategoryId} ({CategoryName}) in {Duration}ms", 
                taskItemViewModels.Count, request.CategoryId, categoryName, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for category-based queries
            if (stopwatch.ElapsedMilliseconds > 600)
            {
                _logger.LogWarning("Slow category task query detected: {Duration}ms for category ID {CategoryId} (threshold: 600ms)", 
                    stopwatch.ElapsedMilliseconds, request.CategoryId);
            }

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} task items for category '{categoryName}' successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve task items for category ID {CategoryId} after {Duration}ms", 
                request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve task items by category: {ex.Message}");
        }
    }
}