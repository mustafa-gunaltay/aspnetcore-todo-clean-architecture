using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetOverdueTaskItems;

public record GetOverdueTaskItemsQuery(
    int UserId) : IRequest<Result<IReadOnlyList<TaskItemViewModel>>>;