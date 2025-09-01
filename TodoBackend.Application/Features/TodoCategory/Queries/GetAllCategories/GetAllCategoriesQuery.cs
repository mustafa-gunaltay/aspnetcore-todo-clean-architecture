using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryViewModel>>>;