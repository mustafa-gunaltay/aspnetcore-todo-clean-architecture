using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public DeleteCategoryCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category exists
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result.Failure("Category not found");
            }

            // Check if category has active tasks
            var hasActiveTasks = await _uow.CategoryRepository.HasActiveTasksAsync(request.CategoryId, cancellationToken);
            if (hasActiveTasks)
            {
                return Result.Failure("Cannot delete category with active tasks");
            }

            // Delete the category (soft delete)
            await _uow.CategoryRepository.DeleteAsync(category, cancellationToken);

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Category deleted successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete category: {ex.Message}");
        }
    }
}