using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public class ValidateUserCredentialsQueryHandler : IRequestHandler<ValidateUserCredentialsQuery, Result<bool>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public ValidateUserCredentialsQueryHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(ValidateUserCredentialsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user by email
            var user = await _uow.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                return Result<bool>.Success(false, "Invalid email or password");
            }

            // Verify password using password hasher
            var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);

            if (isPasswordValid)
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