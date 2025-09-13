using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByTaskItem;

public record GetCategoriesByTaskItemQuery : IRequest<Result<IReadOnlyList<CategorySummaryViewModel>>>
{
    public int TaskItemId { get; init; }

    public GetCategoriesByTaskItemQuery(int taskItemId)
    {
        TaskItemId = taskItemId;
    }
}