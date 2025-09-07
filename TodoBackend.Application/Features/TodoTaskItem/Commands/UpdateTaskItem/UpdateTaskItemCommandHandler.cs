using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;

public class UpdateTaskItemCommandHandler : IRequestHandler<UpdateTaskItemCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public UpdateTaskItemCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if task exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result.Failure("Task not found");
            }

            // ? SELECTIVE UPDATE PATTERN
            
            // Title güncelleme - sadece null de?ilse
            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return Result.Failure("Title cannot be empty");
                }
                taskItem.SetTitle(request.Title);
            }

            // Description güncelleme - explicit flag ile null handling
            if (request.ClearDescription)
            {
                taskItem.SetDescription(null);
            }
            else if (request.Description != null)
            {
                // Empty string'i null'a çevir
                taskItem.SetDescription(string.IsNullOrEmpty(request.Description) ? null : request.Description);
            }
            // request.Description == null && !request.ClearDescription ? De?i?iklik yok

            // Priority güncelleme - sadece null de?ilse
            if (request.Priority.HasValue)
            {
                taskItem.SetPriority(request.Priority.Value);
            }

            // DueDate güncelleme - explicit flag ile null handling
            if (request.ClearDueDate)
            {
                // DueDate'i null yap (ancak Priority High de?ilse)
                if (taskItem.Priority == Domain.Enums.Priority.High)
                {
                    return Result.Failure("Cannot clear due date for high priority tasks");
                }
                taskItem.SetDueDate(null);
            }
            else if (request.DueDate.HasValue)
            {
                taskItem.SetDueDate(request.DueDate.Value);
            }
            // request.DueDate == null && !request.ClearDueDate ? De?i?iklik yok

            // Save changes
            await _uow.TaskItemRepository.UpdateAsync(taskItem, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task updated successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update task: {ex.Message}");
        }
    }
}