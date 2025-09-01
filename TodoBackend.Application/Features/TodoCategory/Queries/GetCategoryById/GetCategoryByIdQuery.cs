using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoryById;

public record GetCategoryByIdQuery(int CategoryId) : IRequest<Result<CategoryViewModel>>;
