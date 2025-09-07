using FluentValidation;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;

public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
{
    public CreateTaskItemCommandValidator()
    {
        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(v => !string.IsNullOrWhiteSpace(v.Description));

        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("Valid UserId is required");

        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Priority must be a valid enum value");

        // Kısıt 10: Yüksek öncelikli görevlerin mutlaka bir Bitiş Tarihi (DueDate) olmalıdır
        RuleFor(v => v.DueDate)
            .NotNull().WithMessage("High priority tasks require a due date")
            .When(v => v.Priority == Priority.High);

        // DueDate gelecek tarih olmalı
        RuleFor(v => v.DueDate)
            .GreaterThan(DateTime.Today).WithMessage("Due date must be in the future")
            .When(v => v.DueDate.HasValue);
    }
}