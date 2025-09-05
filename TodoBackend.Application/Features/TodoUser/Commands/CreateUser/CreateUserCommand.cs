using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoUser.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<int>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}