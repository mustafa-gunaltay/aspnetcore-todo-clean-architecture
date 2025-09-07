using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;

public record DeleteTaskItemCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }

    public DeleteTaskItemCommand(int taskItemId)
    {
        TaskItemId = taskItemId;
    }
}