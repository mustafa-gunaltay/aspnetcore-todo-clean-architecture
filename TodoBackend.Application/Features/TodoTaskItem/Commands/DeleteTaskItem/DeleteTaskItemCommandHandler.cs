using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;

public class DeleteTaskItemCommandHandler : IRequestHandler<DeleteTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public DeleteTaskItemCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if task exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result.Failure("Task not found");
            }
            
            // Ilk olarak task-category ili?kilerini de soft delete et
            await _uow.TaskItemCategoryRepository.DeleteAllByTaskItemIdAsync(request.TaskItemId, cancellationToken);

            await _uow.TaskItemRepository.DeleteAsync(taskItem, cancellationToken);
            
            // Save all changes in one transaction
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task deleted successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete task: {ex.Message}");
        }
    }
}