using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByUserId;

public class GetCategoriesByUserIdQueryValidator : AbstractValidator<GetCategoriesByUserIdQuery>
{
    public GetCategoriesByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");
    }
}
