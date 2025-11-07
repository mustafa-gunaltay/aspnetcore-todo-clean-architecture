using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemById;

public class GetTaskItemByIdQueryValidator : AbstractValidator<GetTaskItemByIdQuery>
{
    public GetTaskItemByIdQueryValidator()
    {
        // TaskItemId zorunlu ve pozitif olmalı
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");
    }
}
