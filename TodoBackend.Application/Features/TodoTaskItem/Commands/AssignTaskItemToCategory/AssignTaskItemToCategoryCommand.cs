using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.AssignTaskItemToCategory;

public record AssignTaskItemToCategoryCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }
    public int CategoryId { get; init; }

    public AssignTaskItemToCategoryCommand(int taskItemId, int categoryId)
    {
        TaskItemId = taskItemId;
        CategoryId = categoryId;
    }
}