using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByFilter;

public class GetTaskItemsByFilterQueryValidator : AbstractValidator<GetTaskItemsByFilterQuery>
{
    public GetTaskItemsByFilterQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.StartDueDate)
            .LessThanOrEqualTo(x => x.EndDueDate)
            .When(x => x.StartDueDate.HasValue && x.EndDueDate.HasValue)
            .WithMessage("StartDueDate must be less than or equal to EndDueDate.");

        RuleFor(x => x.EndDueDate)
            .GreaterThanOrEqualTo(x => x.StartDueDate)
            .When(x => x.StartDueDate.HasValue && x.EndDueDate.HasValue)
            .WithMessage("EndDueDate must be greater than or equal to StartDueDate.");
    }
}
