using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByCategory;

public class GetTaskItemsByCategoryQueryValidator : AbstractValidator<GetTaskItemsByCategoryQuery>
{
    public GetTaskItemsByCategoryQueryValidator()
    {
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
    }
}