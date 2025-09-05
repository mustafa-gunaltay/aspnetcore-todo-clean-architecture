using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoUser.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<Result>
{
    public int UserId { get; init; }
    public string? Email { get; init; } // Nullable - email g�ncellemesi istege bagli
    public string? Password { get; init; } // Nullable - password g�ncellemesi istege bagli
}