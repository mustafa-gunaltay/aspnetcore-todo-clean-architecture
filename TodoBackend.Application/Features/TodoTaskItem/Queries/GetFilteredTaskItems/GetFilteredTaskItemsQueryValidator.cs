using FluentValidation;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetFilteredTaskItems;

public class GetFilteredTaskItemsQueryValidator : AbstractValidator<GetFilteredTaskItemsQuery>
{
    public GetFilteredTaskItemsQueryValidator()
    {
        // UserId zorunlu ve pozitif olmal?
        RuleFor(v => v.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0");

        // Priority varsa enum kontrolü
        RuleFor(v => v.Priority)
            .IsInEnum().WithMessage("Priority must be a valid enum value")
            .When(v => v.Priority.HasValue);

        // CategoryId varsa pozitif olmal?
        RuleFor(v => v.CategoryId)
            .GreaterThan(0).WithMessage("CategoryId must be greater than 0")
            .When(v => v.CategoryId.HasValue);

        // StartDate ve EndDate mant?kl? olmal?
        RuleFor(v => v)
            .Must(v => !v.StartDate.HasValue || !v.EndDate.HasValue || v.StartDate.Value <= v.EndDate.Value)
            .WithMessage("StartDate must be earlier than or equal to EndDate");

        // Tarih aral??? makul s?n?rlar içinde olmal? (performance için)
        RuleFor(v => v)
            .Must(v => !v.StartDate.HasValue || !v.EndDate.HasValue || 
                      (v.EndDate.Value - v.StartDate.Value).TotalDays <= 365)
            .WithMessage("Date range cannot exceed 365 days");

        // StartDate gelecekte çok ileri bir tarih olmamal?
        RuleFor(v => v.StartDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(1))
            .WithMessage("StartDate cannot be more than 1 year in the future")
            .When(v => v.StartDate.HasValue);

        RuleFor(v => v.EndDate)
            .LessThanOrEqualTo(DateTime.Today.AddYears(1))
            .WithMessage("EndDate cannot be more than 1 year in the future")
            .When(v => v.EndDate.HasValue);
    }
}