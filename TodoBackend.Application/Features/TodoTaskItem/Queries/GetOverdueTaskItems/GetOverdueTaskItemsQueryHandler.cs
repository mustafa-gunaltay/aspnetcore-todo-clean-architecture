using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetOverdueTaskItems;

public class GetOverdueTaskItemsQueryHandler : IRequestHandler<GetOverdueTaskItemsQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetOverdueTaskItemsQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetOverdueTaskItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            // Get overdue tasks using repository method
            var overdueTaskItems = await _uow.TaskItemRepository.GetOverdueTasksAsync(
                userId: request.UserId,
                ct: cancellationToken);

            // Map to ViewModels
            var taskItemViewModels = overdueTaskItems.Select(task => new TaskItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId,
                UserEmail = task.User?.Email,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Categories = task.TaskItemCategories
                    ?.Where(tc => !tc.IsDeleted)
                    .Select(tc => new CategorySummaryViewModel
                    {
                        Id = tc.CategoryId,
                        Name = tc.Category?.Name ?? "Unknown"
                    }).ToList() ?? new List<CategorySummaryViewModel>()
            }).ToList();

            return Result<IReadOnlyList<TaskItemViewModel>>.Success(
                taskItemViewModels,
                $"Retrieved {taskItemViewModels.Count} overdue task items successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve overdue task items: {ex.Message}");
        }
    }
}