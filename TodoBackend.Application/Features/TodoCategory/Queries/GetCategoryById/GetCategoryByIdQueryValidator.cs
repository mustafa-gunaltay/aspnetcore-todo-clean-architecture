using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoryById;

public class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        // CategoryId zorunlu ve pozitif olmalı
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0");
    }
}
