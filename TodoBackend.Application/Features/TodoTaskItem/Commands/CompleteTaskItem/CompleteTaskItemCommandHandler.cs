using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CompleteTaskItem;

public class CompleteTaskItemCommandHandler : IRequestHandler<CompleteTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<CompleteTaskItemCommandHandler> _logger;

    public CompleteTaskItemCommandHandler(ITodoBackendUnitOfWork uow, ILogger<CompleteTaskItemCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(CompleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task completion process for ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            // Gereksinim 6a: ITaskItemRepository.GetByIdAsync() - Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task completion failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }

            var taskTitle = taskItem.Title; // Store for logging
            var taskUserId = taskItem.UserId; // Store for logging
            var taskPriority = taskItem.Priority; // Store for logging

            // Check if task is already completed
            if (taskItem.IsCompleted)
            {
                _logger.LogWarning("Task completion failed - task ID {TaskItemId} ({TaskTitle}) is already completed", 
                    request.TaskItemId, taskTitle);
                return Result.Failure("Task is already completed");
            }

            _logger.LogDebug("Found task ID {TaskItemId} ({TaskTitle}) with priority {Priority} for user ID {UserId}, marking as complete", 
                request.TaskItemId, taskTitle, taskPriority, taskUserId);

            // Gereksinim 11: Geçmi? tarihte olan bir görev tamamlanm?? olarak i?aretlenemez
            // Bu kontrol domain model'de Complete() metodunda yap?l?yor
            // Burada ek kontrol yapmaya gerek yok

            // Complete task using domain method
            // Bu method CompletedAt alan?n? otomatik doldurur ve IsCompleted'? true yapar
            taskItem.Complete();

            _logger.LogDebug("Task ID {TaskItemId} marked as completed, CompletedAt set to {CompletedAt}", 
                request.TaskItemId, taskItem.CompletedAt);

            // Gereksinim 6b: ITaskItemRepository.UpdateAsync() - Update task
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            
            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} ({TaskTitle}) completed successfully for user ID {UserId} in {Duration}ms", 
                request.TaskItemId, taskTitle, taskUserId, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow task completion detected: {Duration}ms for task ID {TaskItemId} (threshold: 500ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId);
            }

            return Result.Success("Task completed successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task completion for ID {TaskItemId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task completion failed with exception for ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to complete task: {ex.Message}");
        }
    }
}