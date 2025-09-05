using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetUserById;

public record GetUserByIdQuery(int UserId) : IRequest<Result<UserViewModel>>;