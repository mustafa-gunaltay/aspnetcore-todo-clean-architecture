using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.RemoveTaskItemFromCategory;

public class RemoveTaskItemFromCategoryCommandHandler : IRequestHandler<RemoveTaskItemFromCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public RemoveTaskItemFromCategoryCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(RemoveTaskItemFromCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if task exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result.Failure("Task not found");
            }

            // Check if category exists
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result.Failure("Category not found");
            }

            // Check if task is assigned to this category
            var isAssigned = await _uow.TaskItemCategoryRepository.IsTaskAssignedToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!isAssigned)
            {
                return Result.Failure("Task is not assigned to this category");
            }

            // Remove task from category using repository method
            var removeResult = await _uow.TaskItemCategoryRepository.RemoveTaskFromCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!removeResult)
            {
                return Result.Failure("Failed to remove task from category");
            }

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task removed from category successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to remove task from category: {ex.Message}");
        }
    }
}