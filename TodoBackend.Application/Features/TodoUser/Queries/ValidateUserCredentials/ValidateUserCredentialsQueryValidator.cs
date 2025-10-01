using FluentValidation;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public class ValidateUserCredentialsQueryValidator : AbstractValidator<ValidateUserCredentialsQuery>
{
    private readonly IInputSanitizer _inputSanitizer;

    public ValidateUserCredentialsQueryValidator(IInputSanitizer inputSanitizer)
    {
        _inputSanitizer = inputSanitizer;

        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters")
            .Must(BeSecureEmail).WithMessage("Email contains potentially dangerous characters");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(200).WithMessage("Password must not exceed 200 characters")
            .Must(BeSecureInput).WithMessage("Password contains invalid characters");
    }

    /// <summary>
    /// Email güvenlik kontrolü - XSS ve injection sald?r?lar?n? önler
    /// </summary>
    private bool BeSecureEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return true;

        try
        {
            var sanitized = _inputSanitizer.SanitizeEmail(email);
            return sanitized.Equals(email, StringComparison.OrdinalIgnoreCase);
        }
        catch (ArgumentException)
        {
            // SanitizeEmail invalid format için exception f?rlat?r
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Genel input güvenlik kontrolü - XSS ve SQL injection sald?r?lar?n? önler
    /// </summary>
    private bool BeSecureInput(string input)
    {
        if (string.IsNullOrEmpty(input)) return true;

        try
        {
            var sanitized = _inputSanitizer.SanitizeInput(input);
            return sanitized == input;
        }
        catch
        {
            return false;
        }
    }
}