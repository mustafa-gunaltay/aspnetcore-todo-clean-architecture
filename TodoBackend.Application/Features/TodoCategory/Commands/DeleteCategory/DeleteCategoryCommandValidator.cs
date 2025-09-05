using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        // CategoryId zorunlu ve pozitif olmal?
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
    }
}