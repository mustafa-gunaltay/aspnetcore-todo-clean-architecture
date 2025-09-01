using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryViewModel>>>
{
    private readonly ITodoCleanArchitectureUnitOfWork _uow;

    public GetAllCategoriesQueryHandler(ITodoCleanArchitectureUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CategoryViewModel>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get categories with task count using the repository method
            var categories = await _uow.CategoryRepository.GetCategoriesWithTaskCountAsync(cancellationToken);

            // Map to ViewModels
            var categoryViewModels = categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    TaskCount = c.TaskItemCategories.Count,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList();

            return Result<IReadOnlyList<CategoryViewModel>>.Success(categoryViewModels, "Categories retrieved successfully");
        }
        catch (DomainException dex)
        {
            return Result<IReadOnlyList<CategoryViewModel>>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CategoryViewModel>>.Failure($"Failed to retrieve categories: {ex.Message}");
        }
    }
}