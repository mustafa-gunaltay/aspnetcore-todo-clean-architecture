using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryViewModel>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetCategoryByIdQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<CategoryViewModel>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result<CategoryViewModel>.Failure("Category not found");
            }

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                TaskCount = category.TaskItemCategories.Count,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            return Result<CategoryViewModel>.Success(viewModel, "Category retrieved successfully");
        }
        catch (DomainException dex)
        {
            return Result<CategoryViewModel>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result<CategoryViewModel>.Failure($"Failed to retrieve category: {ex.Message}");
        }
    }
}
