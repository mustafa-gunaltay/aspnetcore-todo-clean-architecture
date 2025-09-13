using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByCategory;

public record GetTaskItemsByCategoryQuery : IRequest<Result<IReadOnlyList<TaskItemViewModel>>>
{
    public int CategoryId { get; init; }

    public GetTaskItemsByCategoryQuery(int categoryId)
    {
        CategoryId = categoryId;
    }
}