using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByTaskItem;

public class GetCategoriesByTaskItemQueryValidator : AbstractValidator<GetCategoriesByTaskItemQuery>
{
    public GetCategoriesByTaskItemQueryValidator()
    {
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");
    }
}