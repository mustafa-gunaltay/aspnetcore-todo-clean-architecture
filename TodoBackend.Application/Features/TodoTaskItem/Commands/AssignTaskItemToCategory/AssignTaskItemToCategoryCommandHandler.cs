using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.AssignTaskItemToCategory;

public class AssignTaskItemToCategoryCommandHandler : IRequestHandler<AssignTaskItemToCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public AssignTaskItemToCategoryCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(AssignTaskItemToCategoryCommand request, CancellationToken cancellationToken)
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

            // Gereksinim 12: Kullan?c? silinmi? kategoriye yeni görev ba?layamamal?d?r
            if (category.IsDeleted)
            {
                return Result.Failure("Cannot assign task to a deleted category");
            }

            // Check if task is already assigned to this category
            var isAlreadyAssigned = await _uow.TaskItemCategoryRepository.IsTaskAssignedToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (isAlreadyAssigned)
            {
                return Result.Failure("Task is already assigned to this category");
            }

            // Assign task to category using repository method
            var assignResult = await _uow.TaskItemCategoryRepository.AssignTaskToCategoryAsync(
                request.TaskItemId, 
                request.CategoryId, 
                cancellationToken);

            if (!assignResult)
            {
                return Result.Failure("Failed to assign task to category");
            }

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Task assigned to category successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to assign task to category: {ex.Message}");
        }
    }
}