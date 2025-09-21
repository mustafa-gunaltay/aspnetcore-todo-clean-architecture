using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.RemoveTaskItemFromCategory;

public class RemoveTaskItemFromCategoryCommandHandler : IRequestHandler<RemoveTaskItemFromCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<RemoveTaskItemFromCategoryCommandHandler> _logger;

    public RemoveTaskItemFromCategoryCommandHandler(ITodoBackendUnitOfWork uow, ILogger<RemoveTaskItemFromCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveTaskItemFromCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task-category removal process for task ID {TaskItemId} from category ID {CategoryId}", 
            request.TaskItemId, request.CategoryId);
        
        try
        {
            // Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task removal failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }

            var taskTitle = taskItem.Title; // Store for logging

            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Task removal failed - category with ID {CategoryId} not found for task ID {TaskItemId}", 
                    request.CategoryId, request.TaskItemId);
                return Result.Failure("Category not found");
            }

            var categoryName = category.Name; // Store for logging

            _logger.LogDebug("Found task ID {TaskItemId} ({TaskTitle}) and category ID {CategoryId} ({CategoryName}), checking assignment", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName);

            // Check if task is assigned to this category
            var isAssigned = await _uow.TaskItemCategoryRepository.IsTaskAssignedToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!isAssigned)
            {
                _logger.LogWarning("Task removal failed - task ID {TaskItemId} ({TaskTitle}) is not assigned to category ID {CategoryId} ({CategoryName})", 
                    request.TaskItemId, taskTitle, request.CategoryId, categoryName);
                return Result.Failure("Task is not assigned to this category");
            }

            // Remove task from category using repository method
            _logger.LogDebug("Removing task ID {TaskItemId} ({TaskTitle}) from category ID {CategoryId} ({CategoryName})", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName);
            var removeResult = await _uow.TaskItemCategoryRepository.RemoveTaskFromCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!removeResult)
            {
                _logger.LogError("Task removal operation failed for task ID {TaskItemId} from category ID {CategoryId}", 
                    request.TaskItemId, request.CategoryId);
                return Result.Failure("Failed to remove task from category");
            }

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} ({TaskTitle}) successfully removed from category ID {CategoryId} ({CategoryName}) in {Duration}ms", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 800)
            {
                _logger.LogWarning("Slow task removal detected: {Duration}ms for task ID {TaskItemId} from category ID {CategoryId} (threshold: 800ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId, request.CategoryId);
            }

            return Result.Success("Task removed from category successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task removal for task ID {TaskItemId} from category ID {CategoryId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, request.CategoryId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task removal failed with exception for task ID {TaskItemId} from category ID {CategoryId} after {Duration}ms", 
                request.TaskItemId, request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to remove task from category: {ex.Message}");
        }
    }
}