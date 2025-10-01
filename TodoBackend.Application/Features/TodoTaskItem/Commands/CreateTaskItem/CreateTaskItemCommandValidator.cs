using FluentValidation;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;

public class CreateTaskItemCommandValidator : AbstractValidator<CreateTaskItemCommand>
{
    private readonly IInputSanitizer _inputSanitizer;

    public CreateTaskItemCommandValidator(IInputSanitizer inputSanitizer)
    {
        _inputSanitizer = inputSanitizer;

        RuleFor(v => v.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .Must(BeSecureHtml).WithMessage("Title contains potentially dangerous content");

        RuleFor(v => v.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .Must(BeSecureHtml).WithMessage("Description contains potentially dangerous content")
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

    /// <summary>
    /// HTML içerik güvenlik kontrolü - XSS saldırılarını önler
    /// </summary>
    private bool BeSecureHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return true;

        try
        {
            var sanitized = _inputSanitizer.SanitizeHtml(input);
            return sanitized == input;
        }
        catch
        {
            return false;
        }
    }
}