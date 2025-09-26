using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;

public record CreateCategoryCommand : IRequest<Result<int>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int UserId { get; init; } // Command'da explicit olarak al?nacak
}
