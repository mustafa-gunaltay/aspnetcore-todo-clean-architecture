using MediatR;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    int CategoryId,
    string Name,
    string? Description
) : IRequest<int>;
