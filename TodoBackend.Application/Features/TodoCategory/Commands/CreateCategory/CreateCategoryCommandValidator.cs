using FluentValidation;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private readonly IInputSanitizer _inputSanitizer;

    public CreateCategoryCommandValidator(IInputSanitizer inputSanitizer)
    {
        _inputSanitizer = inputSanitizer;

        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .Must(BeSecureHtml).WithMessage("Category name contains potentially dangerous content");

        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Category description is required")
            .MaximumLength(500).WithMessage("Category description must not exceed 500 characters")
            .Must(BeSecureHtml).WithMessage("Category description contains potentially dangerous content");

        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("Valid UserId is required");
    }

    /// <summary>
    /// HTML içerik güvenlik kontrolü - XSS sald?r?lar?n? önler
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
