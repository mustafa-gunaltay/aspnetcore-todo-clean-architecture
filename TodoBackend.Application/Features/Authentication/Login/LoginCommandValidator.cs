using FluentValidation;

namespace TodoBackend.Application.Features.Authentication.Login;

/// <summary>
/// Email ve password'ün do?ru formatta olup olmad???n? kontrol eder
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // Email bo? olmamal? ve email format?nda olmal?
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        // Password bo? olmamal? ve en az 6 karakter olmal?
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}