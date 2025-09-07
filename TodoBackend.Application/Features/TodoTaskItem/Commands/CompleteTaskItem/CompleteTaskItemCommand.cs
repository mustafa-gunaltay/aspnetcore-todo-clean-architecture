using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CompleteTaskItem;

public record CompleteTaskItemCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }

    public CompleteTaskItemCommand(int taskItemId)
    {
        TaskItemId = taskItemId;
    }
}