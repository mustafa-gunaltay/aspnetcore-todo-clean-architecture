using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public record DeleteCategoryCommand : IRequest<Result>
{
    public DeleteCategoryCommand(int categoryId)
    {
        CategoryId = categoryId;
    }
    public int CategoryId { get; init; }
}