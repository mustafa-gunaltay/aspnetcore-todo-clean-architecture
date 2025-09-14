using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.Authentication.Login;

/// <summary>
/// Login i?lemini gerçekle?tiren handler
/// 1. Email/password do?rular
/// 2. Do?ruysa user bilgilerini döner (JWT token API katman?nda olu?turulacak)
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<string>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. User'? email ile bul
            var user = await _uow.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return Result<string>.Failure("Invalid email or password");
            }

            // 2. Password'u do?rula
            var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);
            if (!isPasswordValid)
            {
                return Result<string>.Failure("Invalid email or password");
            }

            // 3. User bilgilerini JSON format?nda döndür (API katman?nda JWT token olu?turulacak)
            var userInfo = $"{user.Id}|{user.Email}"; // Simple format: "UserId|Email"

            return Result<string>.Success(userInfo, "Login successful");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Login failed: {ex.Message}");
        }
    }
}