using FluentValidation;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetAllCategories;

public class GetAllCategoriesQueryValidator : AbstractValidator<GetAllCategoriesQuery>
{
    public GetAllCategoriesQueryValidator()
    {
        // GetAllCategoriesQuery parametresiz bir query olduğu için
        // özel bir validation kuralı gerekmiyor.
        // Bu validator sınıfı consistency için oluşturuldu.
        // İleride sayfalama parametreleri eklenirse burada validation yapılabilir.
    }
}
