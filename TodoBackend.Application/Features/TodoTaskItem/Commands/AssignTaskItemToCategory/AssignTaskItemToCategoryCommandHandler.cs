using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.AssignTaskItemToCategory;

public class AssignTaskItemToCategoryCommandHandler : IRequestHandler<AssignTaskItemToCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<AssignTaskItemToCategoryCommandHandler> _logger;

    public AssignTaskItemToCategoryCommandHandler(ITodoBackendUnitOfWork uow, ILogger<AssignTaskItemToCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(AssignTaskItemToCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task-category assignment process for task ID {TaskItemId} to category ID {CategoryId}", 
            request.TaskItemId, request.CategoryId);
        
        try
        {
            // Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task assignment failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }

            var taskTitle = taskItem.Title; // Store for logging

            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Task assignment failed - category with ID {CategoryId} not found for task ID {TaskItemId}", 
                    request.CategoryId, request.TaskItemId);
                return Result.Failure("Category not found");
            }

            var categoryName = category.Name; // Store for logging

            // Gereksinim 12: Kullan?c? silinmi? kategoriye yeni görev ba?layamamal?d?r
            if (category.IsDeleted)
            {
                _logger.LogWarning("Task assignment failed - cannot assign task ID {TaskItemId} to deleted category ID {CategoryId} ({CategoryName})", 
                    request.TaskItemId, request.CategoryId, categoryName);
                return Result.Failure("Cannot assign task to a deleted category");
            }

            _logger.LogDebug("Found task ID {TaskItemId} ({TaskTitle}) and category ID {CategoryId} ({CategoryName}), checking existing assignment", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName);

            // Check if task is already assigned to this category
            var isAlreadyAssigned = await _uow.TaskItemCategoryRepository.IsTaskAssignedToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (isAlreadyAssigned)
            {
                _logger.LogWarning("Task assignment failed - task ID {TaskItemId} ({TaskTitle}) is already assigned to category ID {CategoryId} ({CategoryName})", 
                    request.TaskItemId, taskTitle, request.CategoryId, categoryName);
                return Result.Failure("Task is already assigned to this category");
            }

            // Assign task to category using repository method
            _logger.LogDebug("Assigning task ID {TaskItemId} ({TaskTitle}) to category ID {CategoryId} ({CategoryName})", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName);
            var assignResult = await _uow.TaskItemCategoryRepository.AssignTaskToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!assignResult)
            {
                _logger.LogError("Task assignment operation failed for task ID {TaskItemId} to category ID {CategoryId}", 
                    request.TaskItemId, request.CategoryId);
                return Result.Failure("Failed to assign task to category");
            }

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} ({TaskTitle}) successfully assigned to category ID {CategoryId} ({CategoryName}) in {Duration}ms", 
                request.TaskItemId, taskTitle, request.CategoryId, categoryName, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 800)
            {
                _logger.LogWarning("Slow task assignment detected: {Duration}ms for task ID {TaskItemId} to category ID {CategoryId} (threshold: 800ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId, request.CategoryId);
            }

            return Result.Success("Task assigned to category successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task assignment for task ID {TaskItemId} to category ID {CategoryId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, request.CategoryId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task assignment failed with exception for task ID {TaskItemId} to category ID {CategoryId} after {Duration}ms", 
                request.TaskItemId, request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to assign task to category: {ex.Message}");
        }
    }
}