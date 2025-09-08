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

    public LoginCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<string>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Email/password'ü veritaban?nda kontrol et
            var isValidUser = await _uow.UserRepository.ValidateCredentialsAsync(
                request.Email, 
                request.Password, 
                cancellationToken);

            if (!isValidUser)
            {
                return Result<string>.Failure("Invalid email or password");
            }

            // 2. User bilgilerini al
            var user = await _uow.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return Result<string>.Failure("User not found");
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