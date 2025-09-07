using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.AssignTaskItemToCategory;

public class AssignTaskItemToCategoryCommandValidator : AbstractValidator<AssignTaskItemToCategoryCommand>
{
    public AssignTaskItemToCategoryCommandValidator()
    {
        // TaskItemId zorunlu ve pozitif olmal?
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");

        // CategoryId zorunlu ve pozitif olmal?
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
    }
}