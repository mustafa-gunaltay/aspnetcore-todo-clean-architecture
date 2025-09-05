using FluentValidation;

namespace TodoBackend.Application.Features.TodoUser.Commands.DeleteUser;

public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}