using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        
    }
}