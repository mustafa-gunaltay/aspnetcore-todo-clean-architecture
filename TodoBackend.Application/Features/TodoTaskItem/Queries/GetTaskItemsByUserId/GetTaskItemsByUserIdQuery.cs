using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByUserId;

public record GetTaskItemsByUserIdQuery(
    int UserId) : IRequest<Result<IReadOnlyList<TaskItemViewModel>>>;