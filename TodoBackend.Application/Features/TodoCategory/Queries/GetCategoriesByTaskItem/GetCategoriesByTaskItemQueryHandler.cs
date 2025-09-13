using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByTaskItem;

public class GetCategoriesByTaskItemQueryHandler : IRequestHandler<GetCategoriesByTaskItemQuery, Result<IReadOnlyList<CategorySummaryViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetCategoriesByTaskItemQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<CategorySummaryViewModel>>> Handle(GetCategoriesByTaskItemQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if task item exists
            var taskItem = await _uow.TaskItemRepository.GetByIdAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                return Result<IReadOnlyList<CategorySummaryViewModel>>.Failure("Task item not found");
            }

            // Get TaskItemCategory relations for this task item
            var taskItemCategories = await _uow.TaskItemCategoryRepository.GetCategoriesByTaskItemIdAsync(request.TaskItemId, cancellationToken);

            // Map to CategorySummaryViewModels
            var categorySummaryViewModels = taskItemCategories.Select(tic => new CategorySummaryViewModel
            {
                Id = tic.Category.Id,
                Name = tic.Category.Name
            }).ToList();

            return Result<IReadOnlyList<CategorySummaryViewModel>>.Success(
                categorySummaryViewModels,
                $"Retrieved {categorySummaryViewModels.Count} categories for task item '{taskItem.Title}' successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CategorySummaryViewModel>>.Failure($"Failed to retrieve categories by task item: {ex.Message}");
        }
    }
}