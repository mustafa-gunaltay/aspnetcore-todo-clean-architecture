using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByUserId;

public record GetCategoriesByUserIdQuery(int UserId) : IRequest<Result<IReadOnlyList<CategoryViewModel>>>;


