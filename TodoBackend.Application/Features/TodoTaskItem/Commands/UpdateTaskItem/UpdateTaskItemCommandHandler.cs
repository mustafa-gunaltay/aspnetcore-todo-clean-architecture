using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;

public class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<UpdateTaskItemCommandHandler> _logger;

    public UpdateTaskItemCommandHandler(ITodoBackendUnitOfWork uow, ILogger<UpdateTaskItemCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task update process for ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            // Check if task exists
            _logger.LogDebug("Checking if task with ID {TaskItemId} exists", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                _logger.LogWarning("Task update failed - task with ID {TaskItemId} not found", request.TaskItemId);
                return Result.Failure("Task not found");
            }

            var oldTitle = taskItem.Title;
            var oldDescription = taskItem.Description;
            var oldPriority = taskItem.Priority;
            var oldDueDate = taskItem.DueDate;
            var changes = new List<string>();

            _logger.LogDebug("Found task ID {TaskItemId} with title {TaskTitle}, applying selective updates", 
                request.TaskItemId, oldTitle);

            // SELECTIVE UPDATE PATTERN

            // Title güncelleme - sadece null değilse
            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    _logger.LogWarning("Task update failed - empty title provided for task ID {TaskItemId}", request.TaskItemId);
                    return Result.Failure("Title cannot be empty");
                }
                taskItem.SetTitle(request.Title);
                changes.Add($"Title: '{oldTitle}' -> '{request.Title}'");
            }

            // Description güncelleme - explicit flag ile null handling
            if (request.ClearDescription)
            {
                taskItem.SetDescription(null);
                changes.Add($"Description: '{oldDescription}' -> [CLEARED]");
            }
            else if (request.Description != null)
            {
                // Empty string'i null'a çevir
                var newDescription = string.IsNullOrEmpty(request.Description) ? null : request.Description;
                taskItem.SetDescription(newDescription);
                changes.Add($"Description: '{oldDescription}' -> '{newDescription}'");
            }

            // Priority güncelleme - sadece null değilse
            if (request.Priority.HasValue)
            {
                taskItem.SetPriority(request.Priority.Value);
                changes.Add($"Priority: {oldPriority} -> {request.Priority.Value}");
            }

            // DueDate güncelleme - explicit flag ile null handling
            if (request.ClearDueDate)
            {
                // DueDate'i null yap (ancak Priority High değilse)
                if (taskItem.Priority == Domain.Enums.Priority.High)
                {
                    _logger.LogWarning("Task update failed - cannot clear due date for high priority task ID {TaskItemId}", request.TaskItemId);
                    return Result.Failure("Cannot clear due date for high priority tasks");
                }
                taskItem.SetDueDate(null);
                changes.Add($"DueDate: {oldDueDate} -> [CLEARED]");
            }
            else if (request.DueDate.HasValue)
            {
                taskItem.SetDueDate(request.DueDate.Value);
                changes.Add($"DueDate: {oldDueDate} -> {request.DueDate.Value}");
            }

            _logger.LogDebug("Updating task ID {TaskItemId} with changes: {Changes}", 
                request.TaskItemId, string.Join(", ", changes));

            // Save changes
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task ID {TaskItemId} updated successfully in {Duration}ms with changes: {Changes}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, string.Join(", ", changes));

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow task update detected: {Duration}ms for task ID {TaskItemId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.TaskItemId);
            }

            return Result.Success("Task updated successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task update for ID {TaskItemId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task update failed with exception for ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to update task: {ex.Message}");
        }
    }
}