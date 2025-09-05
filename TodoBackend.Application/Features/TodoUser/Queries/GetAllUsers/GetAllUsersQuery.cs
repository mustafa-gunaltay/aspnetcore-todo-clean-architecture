using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetAllUsers;

public record GetAllUsersQuery : IRequest<Result<IReadOnlyList<UserViewModel>>>;