using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public record ValidateUserCredentialsQuery : IRequest<Result<bool>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}