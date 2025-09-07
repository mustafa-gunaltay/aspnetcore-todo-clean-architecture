using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.RemoveTaskItemFromCategory;

public record RemoveTaskItemFromCategoryCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }
    public int CategoryId { get; init; }

    public RemoveTaskItemFromCategoryCommand(int taskItemId, int categoryId)
    {
        TaskItemId = taskItemId;
        CategoryId = categoryId;
    }
}