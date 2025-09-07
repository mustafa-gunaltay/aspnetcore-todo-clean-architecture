using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.DeleteTaskItem;

public class DeleteTaskItemCommandValidator : AbstractValidator<DeleteTaskItemCommand>
{
    public DeleteTaskItemCommandValidator()
    {
        // TaskItemId zorunlu ve pozitif olmal?
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");
    }
}