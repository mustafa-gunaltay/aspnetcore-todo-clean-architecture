using FluentValidation;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetAllUsers;

public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
{
    public GetAllUsersQueryValidator()
    {
        // GetAllUsersQuery parametresiz bir query oldugu i�in
        // �zel bir validation kural gerekmiyor.
        // Bu validator sinif consistency i�in olusturuldu.
        // ileride sayfalama parametreleri eklenirse burada validation yapilabilir.
    }
}