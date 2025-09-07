using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CompleteTaskItem;

public class CompleteTaskItemCommandHandler : IRequestHandler<CompleteTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public CompleteTaskItemCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(CompleteTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Gereksinim 6a: ITaskItemRepository.GetByIdAsync() - Check if task exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result.Failure("Task not found");
            }

            // Check if task is already completed
            if (taskItem.IsCompleted)
            {
                return Result.Failure("Task is already completed");
            }

            // Gereksinim 11: Geçmi? tarihte olan bir görev tamamlanm?? olarak i?aretlenemez
            // Bu kontrol domain model'de Complete() metodunda yap?l?yor
            // Burada ek kontrol yapmaya gerek yok

            // Complete task using domain method
            // Bu method CompletedAt alan?n? otomatik doldurur ve IsCompleted'? true yapar
            taskItem.Complete();

            // Gereksinim 6b: ITaskItemRepository.UpdateAsync() - Update task
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            
            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task completed successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to complete task: {ex.Message}");
        }
    }
}