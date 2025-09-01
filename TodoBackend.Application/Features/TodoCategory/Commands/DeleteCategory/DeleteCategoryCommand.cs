using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public record DeleteCategoryCommand : IRequest<Result>
{
    public int CategoryId { get; init; }
}