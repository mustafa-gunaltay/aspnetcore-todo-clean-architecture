using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoUser.Commands.DeleteUser;

public record DeleteUserCommand : IRequest<Result>
{
    public int UserId { get; init; }

    public DeleteUserCommand(int userId)
    {
        UserId = userId;
    }
}