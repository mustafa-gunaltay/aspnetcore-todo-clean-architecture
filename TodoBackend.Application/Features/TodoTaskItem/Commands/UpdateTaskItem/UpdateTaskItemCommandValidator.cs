using FluentValidation;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;

public class UpdateTaskItemCommandValidator : AbstractValidator<UpdateTaskItemCommand>
{
    public UpdateTaskItemCommandValidator()
    {
        // TaskItemId zorunlu ve pozitif olmal?
        RuleFor(v => v.TaskItemId)
            .GreaterThan(0).WithMessage("TaskItemId must be greater than 0");

        // Title varsa validasyon yap (null = de?i?iklik yok)
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title cannot be empty")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .When(v => v.Title != null);

        // Description varsa validasyon yap (null = de?i?iklik yok, clearDescription flag ayr?ca kontrol edilir)
        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(v => v.Description != null && !string.IsNullOrEmpty(v.Description));

        // Priority varsa enum kontrolü (null = de?i?iklik yok)
        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Priority must be a valid enum value")
            .When(v => v.Priority.HasValue);

        // K?s?t 10: Yüksek öncelikli görevlerin mutlaka bir Biti? Tarihi (DueDate) olmal?d?r
        RuleFor(v => v)
            .Must(v => !(v.Priority == Priority.High && v.ClearDueDate))
            .WithMessage("High priority tasks cannot have cleared due date")
            .When(v => v.Priority == Priority.High);

        RuleFor(v => v)
            .Must(v => !(v.Priority == Priority.High && !v.DueDate.HasValue && !v.ClearDueDate))
            .WithMessage("High priority tasks require a due date")
            .When(v => v.Priority == Priority.High);

        // DueDate gelecek tarih olmal? (sadece de?er verilmi?se)
        RuleFor(v => v.DueDate)
            .GreaterThan(DateTime.Today).WithMessage("Due date must be in the future")
            .When(v => v.DueDate.HasValue);

        // ClearDueDate ve DueDate ayn? anda set edilemez
        RuleFor(v => v)
            .Must(v => !(v.ClearDueDate && v.DueDate.HasValue))
            .WithMessage("Cannot clear due date and set a new due date at the same time");

        // ClearDescription ve Description ayn? anda set edilemez
        RuleFor(v => v)
            .Must(v => !(v.ClearDescription && !string.IsNullOrEmpty(v.Description)))
            .WithMessage("Cannot clear description and set a new description at the same time");
    }
}