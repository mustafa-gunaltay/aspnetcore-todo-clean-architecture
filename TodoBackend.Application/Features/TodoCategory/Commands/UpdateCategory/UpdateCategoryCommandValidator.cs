using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        // CategoryId zorunlu ve pozitif olmal?
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");

        // UserId zorunlu ve pozitif olmal? - security için
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("Valid UserId is required");

        // Name varsa validasyon yap (null = de?i?iklik yok)
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Category name cannot be empty")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Category name cannot be empty or whitespace")
            .When(v => v.Name != null);

        // Description varsa validasyon yap (null = de?i?iklik yok)
        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Category description cannot be empty")
            .MaximumLength(400).WithMessage("Category description must not exceed 400 characters")
            .Must(desc => !string.IsNullOrWhiteSpace(desc)).WithMessage("Category description cannot be empty or whitespace")
            .When(v => v.Description != null);
    }
}
