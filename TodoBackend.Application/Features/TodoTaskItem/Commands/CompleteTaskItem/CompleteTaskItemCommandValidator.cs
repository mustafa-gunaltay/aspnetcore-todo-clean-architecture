using FluentValidation;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CompleteTaskItem;

public class CompleteTaskItemCommandValidator : AbstractValidator<CompleteTaskItemCommand>
{
    public CompleteTaskItemCommandValidator()
    {
        // TaskItemId zorunlu ve pozitif olmal?
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");

        // NOT: Gereksinim 11 (Ge�mi? tarihte olan bir g�rev tamamlanm?? olarak i?aretlenemez) 
        // kontrol� domain model'de Complete() metodunda yap?l?yor.
        // Validation layer'da de?il, domain layer'da kontrol edilir ��nk�:
        // 1. DueDate business logic'e ba?l?
        // 2. Domain invariant'lar? domain model'de korunmal?
        // 3. Validator sadece basic input validation yapmal?
    }
}