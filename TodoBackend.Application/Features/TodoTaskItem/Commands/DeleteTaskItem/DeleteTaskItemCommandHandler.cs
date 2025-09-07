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

            // Gereksinim 5: Kullan?c? kendisine ait görevleri silebilmelidir
            // TaskItem üstünde UserId null i?aretlendi?inde TaskItem'in varl???n?n bir anlam? kalmaz 
            // dolay?s?yla soft delete i?lemi de birlikte yap?l?r
            
            // ?lk olarak task-category ili?kilerini de soft delete et
            await _uow.TaskItemCategoryRepository.DeleteAllByTaskItemIdAsync(request.TaskItemId, cancellationToken);

            // TaskItem'? user ile ili?kisini keserek soft delete et
            var deleteResult = await _uow.TaskItemRepository.DeleteUserFromTaskAsync(request.TaskItemId, cancellationToken);
            
            if (!deleteResult)
            {
                return Result.Failure("Failed to delete task or task is already deleted");
            }

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