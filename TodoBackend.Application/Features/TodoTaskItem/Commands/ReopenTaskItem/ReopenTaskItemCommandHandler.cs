using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.ReopenTaskItem;

public class ReopenTaskItemCommandHandler : IRequestHandler<ReopenTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public ReopenTaskItemCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(ReopenTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Gereksinim 7a: ITaskItemRepository.GetByIdAsync() - Check if task exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result.Failure("Task not found");
            }

            // Check if task is not completed (already reopened)
            if (!taskItem.IsCompleted)
            {
                return Result.Failure("Task is not completed, cannot reopen");
            }

            // Reopen task using domain method
            // Bu method IsCompleted'? false yapar ve CompletedAt'? null yapar
            taskItem.Reopen();

            // Gereksinim 7b: ITaskItemRepository.UpdateAsync() - Update task
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            
            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task reopened successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to reopen task: {ex.Message}");
        }
    }
}