using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByCategory;

public class GetTaskItemsByCategoryQueryHandler : IRequestHandler<GetTaskItemsByCategoryQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetTaskItemsByCategoryQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetTaskItemsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category exists
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("Category not found");
            }

            // Get TaskItemCategory relations for this category
            var taskItemCategories = await _uow.TaskItemCategoryRepository.GetTaskItemsByCategoryIdAsync(request.CategoryId, cancellationToken);

            // Map to TaskItemViewModels
            var taskItemViewModels = taskItemCategories.Select(tic => new TaskItemViewModel
            {
                Id = tic.TaskItem.Id,
                Title = tic.TaskItem.Title,
                Description = tic.TaskItem.Description,
                Priority = tic.TaskItem.Priority,
                DueDate = tic.TaskItem.DueDate,
                CompletedAt = tic.TaskItem.CompletedAt,
                IsCompleted = tic.TaskItem.IsCompleted,
                UserId = tic.TaskItem.UserId,
                UserEmail = tic.TaskItem.User?.Email,
                CreatedAt = tic.TaskItem.CreatedAt,
                UpdatedAt = tic.TaskItem.UpdatedAt,
                Categories = tic.TaskItem.TaskItemCategories
                    ?.Where(tc => !tc.IsDeleted)
                    .Select(tc => new CategorySummaryViewModel
                    {
                        Id = tc.CategoryId,
                        Name = tc.Category?.Name ?? "Unknown"
                    }).ToList() ?? new List<CategorySummaryViewModel>()
            }).ToList();

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} task items for category '{category.Name}' successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve task items by category: {ex.Message}");
        }
    }
}