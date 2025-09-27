using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;

public class DeleteTaskItemCommandHandler : IRequestHandler<DeleteTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<DeleteTaskItemCommandHandler> _logger;

    public DeleteTaskItemCommandHandler(ITodoBackendUnitOfWork uow, ILogger<DeleteTaskItemCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task deletion process for ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            // Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task deletion failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }
            
            var taskTitle = taskItem.Title; // Store for logging
            var taskUserId = taskItem.UserId; // Store for logging
            
            _logger.LogDebug("Found task ID {TaskItemId} with title {TaskTitle} (User ID: {UserId}), deleting task-category relationships", 
                request.TaskItemId, taskTitle, taskUserId);

            // İlk olarak task-category ilişkilerini de soft delete et
            _logger.LogDebug("Deleting task-category relationships for task ID {TaskItemId}", request.TaskItemId);
            await _uow.TaskItemCategoryRepository.DeleteAllByTaskItemIdAsync(request.TaskItemId, cancellationToken);
            

            // Task'ı soft delete et
            _logger.LogDebug("Performing soft delete for task ID {TaskItemId} ({TaskTitle})", 
                request.TaskItemId, taskTitle);
            await _uow.TaskItemRepository.DeleteAsync(taskItem, cancellationToken);
            
            // Save all changes in one transaction
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} ({TaskTitle}) and its category relationships deleted successfully for user ID {UserId} in {Duration}ms", 
                request.TaskItemId, taskTitle, taskUserId, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow task deletion detected: {Duration}ms for task ID {TaskItemId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId);
            }

            return Result.Success("Task deleted successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task deletion for ID {TaskItemId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task deletion failed with exception for ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to delete task: {ex.Message}");
        }
    }
}