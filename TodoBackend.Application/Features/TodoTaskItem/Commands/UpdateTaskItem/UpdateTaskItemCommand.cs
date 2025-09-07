using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.UpdateTaskItem;

public record UpdateTaskItemCommand : IRequest<Result>
{
    public int TaskItemId { get; init; }
    public string? Title { get; init; }
    public string? Description { get; init; }
    public Priority? Priority { get; init; }
    public DateTime? DueDate { get; init; }
    public bool ClearDueDate { get; init; } = false; // DueDate'i null yapmak için explicit flag
    public bool ClearDescription { get; init; } = false; // Description'i null yapmak için explicit flag
}

/* Notlar:
var command = new UpdateTaskItemCommand 
{
    TaskItemId = 1,
    DueDate = null  // Bu ne demek?
};

Iki farklı anlam olabilirdi:
1.	"DueDate'i değiştirme" (mevcut değeri koru)
2.	"DueDate'i null yap" (tarihi temizle)

ClearDueDate flag'i ile bu karışıklığı önlüyoruz:
- ClearDueDate = true ise DueDate null yapılacak
- ClearDueDate = false ise DueDate değiştirilmemiş olacak
*/

/*
Selective Update Pattern:
- null değerler = "Bu alanı değiştirme" (mevcut değeri koru)
- dolu değerler = "Bu alanı güncelle"
- explicit flags = "Bu alanı null yap"
*/