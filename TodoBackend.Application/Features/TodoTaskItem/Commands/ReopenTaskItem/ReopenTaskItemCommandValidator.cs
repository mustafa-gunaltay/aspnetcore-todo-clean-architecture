using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.ReopenTaskItem;

public class ReopenTaskItemCommandValidator : AbstractValidator<ReopenTaskItemCommand>
{
    public ReopenTaskItemCommandValidator()
    {
        // TaskItemId zorunlu ve pozitif olmal?
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");

        // NOT: Reopen i?lemi için özel business rule kontrolü gerekmiyor
        // Sadece task'?n tamamlanm?? olmas? yeterli, bu kontrol handler'da yap?l?yor
        // Domain model'de Reopen() metodu idempotent olarak tasarlanm??
    }
}