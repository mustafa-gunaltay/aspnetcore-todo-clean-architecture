using FluentValidation;

namespace TodoBackend.Application.Features.TodoUser.Commands.UpdateUser;

public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");

        RuleFor(v => v.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters")
            .When(v => !string.IsNullOrWhiteSpace(v.Email)); // Sadece password verilmisse validate et

        RuleFor(v => v.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(200).WithMessage("Password must not exceed 200 characters")
            .When(v => !string.IsNullOrWhiteSpace(v.Password)); // Sadece password verilmisse validate et
    }
}