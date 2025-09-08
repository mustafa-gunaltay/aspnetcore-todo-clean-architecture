using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetUpcomingTaskItems;

public record GetUpcomingTaskItemsQuery(
    int UserId,
    int Days = 7) : IRequest<Result<IReadOnlyList<TaskItemViewModel>>>;