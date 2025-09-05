using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public class ValidateUserCredentialsQueryHandler : IRequestHandler<ValidateUserCredentialsQuery, Result<bool>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public ValidateUserCredentialsQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<bool>> Handle(ValidateUserCredentialsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate credentials using repository method
            var isValid = await _uow.UserRepository.ValidateCredentialsAsync(
                request.Email, 
                request.Password, 
                cancellationToken);

            if (isValid)
            {
                return Result<bool>.Success(true, "Credentials are valid");
            }
            else
            {
                return Result<bool>.Success(false, "Invalid email or password");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to validate credentials: {ex.Message}");
        }
    }
}