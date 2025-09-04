using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public CreateCategoryCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category name is unique
            var isNameUnique = await _uow.CategoryRepository.IsNameUniqueAsync(request.Name, null, cancellationToken);
            if (!isNameUnique)
            {
                return Result<int>.Failure($"Category with name '{request.Name}' already exists");
            }

            // Create new category
            var category = new Category(request.Name, request.Description);

            // Save to database
            await _uow.CategoryRepository.AddAsync(category, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken); // Save changes to the database

            // Return success with the new category's ID
            return Result<int>.Success(category.Id, "Category created successfully");
        }
        catch (DomainException ex)
        {
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
    }
}
