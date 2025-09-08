using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByUserId;

public class GetTaskItemsByUserIdQueryHandler : IRequestHandler<GetTaskItemsByUserIdQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetTaskItemsByUserIdQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetTaskItemsByUserIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            // Get tasks by user id using repository method
            var taskItems = await _uow.TaskItemRepository.GetTasksByUserIdAsync(
                userId: request.UserId,
                ct: cancellationToken);

            // Map to ViewModels
            var taskItemViewModels = taskItems.Select(task => new TaskItemViewModel
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
                $"Retrieved {taskItemViewModels.Count} task items for user successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve task items by user id: {ex.Message}");
        }
    }
}