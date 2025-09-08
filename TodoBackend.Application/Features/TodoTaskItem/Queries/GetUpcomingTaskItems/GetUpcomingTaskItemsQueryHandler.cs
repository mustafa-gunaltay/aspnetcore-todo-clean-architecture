using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetUpcomingTaskItems;

public class GetUpcomingTaskItemsQueryHandler : IRequestHandler<GetUpcomingTaskItemsQuery, Result<IReadOnlyList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetUpcomingTaskItemsQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<TaskItemViewModel>>> Handle(GetUpcomingTaskItemsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<IReadOnlyList<TaskItemViewModel>>.Failure("User not found");
            }

            // Get upcoming tasks using repository method
            var upcomingTaskItems = await _uow.TaskItemRepository.GetUpcomingTasksAsync(
                userId: request.UserId,
                days: request.Days,
                ct: cancellationToken);

            // Map to ViewModels
            var taskItemViewModels = upcomingTaskItems.Select(task => new TaskItemViewModel
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
                $"Retrieved {taskItemViewModels.Count} upcoming task items for the next {request.Days} days successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<TaskItemViewModel>>.Failure($"Failed to retrieve upcoming task items: {ex.Message}");
        }
    }
}