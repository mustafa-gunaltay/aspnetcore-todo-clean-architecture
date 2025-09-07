using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.ReopenTaskItem;

public record ReopenTaskItemCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }

    public ReopenTaskItemCommand(int taskItemId)
    {
        TaskItemId = taskItemId;
    }
}