using FluentValidation;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetUserById;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}