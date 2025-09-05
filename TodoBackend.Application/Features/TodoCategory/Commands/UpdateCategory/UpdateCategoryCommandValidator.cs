using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        // CategoryId zorunlu ve pozitif olmal?
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");

        // Name zorunlu ve uzunluk k?s?tlamalar? (Database: NVARCHAR(100))
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Category name cannot be empty or whitespace");

        // Description zorunlu ve uzunluk k?s?tlamalar? (Database: NVARCHAR(400))
        RuleFor(v => v.Description)
            .NotEmpty().WithMessage("Category description is required")
            .MaximumLength(400).WithMessage("Category description must not exceed 400 characters")
            .Must(desc => !string.IsNullOrWhiteSpace(desc)).WithMessage("Category description cannot be empty or whitespace");
    }
}
