using FluentValidation;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public class ValidateUserCredentialsQueryValidator : AbstractValidator<ValidateUserCredentialsQuery>
{
    public ValidateUserCredentialsQueryValidator()
    {
        RuleFor(v => v.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(v => v.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(200).WithMessage("Password must not exceed 200 characters");
    }
}