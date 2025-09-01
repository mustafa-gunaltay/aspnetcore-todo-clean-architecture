using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ITodoCleanArchitectureUnitOfWork _uow;

    public UpdateCategoryCommandHandler(ITodoCleanArchitectureUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category exists
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result.Failure("Category not found");
            }

            // Check if new name is unique (exclude current category)
            var isNameUnique = await _uow.CategoryRepository.IsNameUniqueAsync(request.Name, request.CategoryId, cancellationToken);
            if (!isNameUnique)
            {
                return Result.Failure("Category name must be unique");
            }

            // Update properties using domain methods
            category.Rename(request.Name);
            category.SetDescription(request.Description);

            // Save changes
            await _uow.CategoryRepository.UpdateAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("Category updated successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update category: {ex.Message}");
        }
    }
}
