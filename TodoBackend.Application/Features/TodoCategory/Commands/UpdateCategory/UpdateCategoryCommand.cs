using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public record UpdateCategoryCommand : IRequest<Result>
{
    public int CategoryId { get; init; }
    public int UserId { get; init; } // Kategorinin ait oldu?u user - security i�in
    public string? Name { get; init; } // null = de?i?iklik yok
    public string? Description { get; init; } // null = de?i?iklik yok
}

/* Notlar:
Selective Update Pattern:
- null de?erler = "Bu alan? de?i?tirme" (mevcut de?eri koru)
- dolu de?erler = "Bu alan? g�ncelle"

�rnek kullan?m:
var command = new UpdateCategoryCommand 
{
    CategoryId = 1,
    UserId = 123,
    Name = "New Name",  // Bu alan g�ncellenecek
    Description = null  // Bu alan de?i?tirilmeyecek (mevcut de?er korunacak)
};
*/
