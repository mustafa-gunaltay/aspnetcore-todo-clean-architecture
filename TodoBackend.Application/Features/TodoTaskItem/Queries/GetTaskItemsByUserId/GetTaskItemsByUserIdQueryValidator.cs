using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByUserId;

public class GetTaskItemsByUserIdQueryValidator : AbstractValidator<GetTaskItemsByUserIdQuery>
{
    public GetTaskItemsByUserIdQueryValidator()
    {
        // UserId zorunlu ve pozitif olmal?
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}