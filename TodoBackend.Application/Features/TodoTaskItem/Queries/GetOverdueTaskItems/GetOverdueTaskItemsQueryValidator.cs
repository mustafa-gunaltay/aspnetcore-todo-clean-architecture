using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetOverdueTaskItems;

public class GetOverdueTaskItemsQueryValidator : AbstractValidator<GetOverdueTaskItemsQuery>
{
    public GetOverdueTaskItemsQueryValidator()
    {
        // UserId zorunlu ve pozitif olmal?
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");
    }
}