using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.ReopenTaskItem;

public class ReopenTaskItemCommandHandler : IRequestHandler<ReopenTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<ReopenTaskItemCommandHandler> _logger;

    public ReopenTaskItemCommandHandler(ITodoBackendUnitOfWork uow, ILogger<ReopenTaskItemCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(ReopenTaskItemCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task reopening process for ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            // Gereksinim 7a: ITaskItemRepository.GetByIdAsync() - Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task reopening failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }

            var taskTitle = taskItem.Title; // Store for logging
            var taskUserId = taskItem.UserId; // Store for logging
            var taskPriority = taskItem.Priority; // Store for logging
            var completedAt = taskItem.CompletedAt; // Store for logging

            // Check if task is not completed (already reopened)
            if (!taskItem.IsCompleted)
            {
                _logger.LogWarning("Task reopening failed - task ID {TaskItemId} ({TaskTitle}) is not completed, cannot reopen", 
                    request.TaskItemId, taskTitle);
                return Result.Failure("Task is not completed, cannot reopen");
            }

            _logger.LogDebug("Found completed task ID {TaskItemId} ({TaskTitle}) with priority {Priority} for user ID {UserId}, completed at {CompletedAt}, reopening", 
                request.TaskItemId, taskTitle, taskPriority, taskUserId, completedAt);

            // Reopen task using domain method
            // Bu method IsCompleted'? false yapar ve CompletedAt'? null yapar
            taskItem.Reopen();

            _logger.LogDebug("Task ID {TaskItemId} reopened, IsCompleted set to false and CompletedAt cleared", 
                request.TaskItemId);

            // Gereksinim 7b: ITaskItemRepository.UpdateAsync() - Update task
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            
            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} ({TaskTitle}) reopened successfully for user ID {UserId} in {Duration}ms", 
                request.TaskItemId, taskTitle, taskUserId, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow task reopening detected: {Duration}ms for task ID {TaskItemId} (threshold: 500ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId);
            }

            return Result.Success("Task reopened successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task reopening for ID {TaskItemId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task reopening failed with exception for ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to reopen task: {ex.Message}");
        }
    }
}