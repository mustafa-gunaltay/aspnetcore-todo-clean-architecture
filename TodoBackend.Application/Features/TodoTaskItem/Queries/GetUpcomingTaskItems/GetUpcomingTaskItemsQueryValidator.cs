using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetUpcomingTaskItems;

public class GetUpcomingTaskItemsQueryValidator : AbstractValidator<GetUpcomingTaskItemsQuery>
{
    public GetUpcomingTaskItemsQueryValidator()
    {
        // UserId zorunlu ve pozitif olmal?
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");

        // Days parametresi pozitif ve makul bir aral?kta olmal?
        RuleFor(v => v.Days)
            .GreaterThan(0).WithMessage("Days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Days cannot exceed 365");
    }
}