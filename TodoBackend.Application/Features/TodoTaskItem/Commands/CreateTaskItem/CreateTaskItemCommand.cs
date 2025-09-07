using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;

public record CreateTaskItemCommand : IRequest<Result<int>>
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Priority Priority { get; init; } = Priority.Medium;
    public DateTime? DueDate { get; init; }
    public int UserId { get; init; }// Command'da explicit olarak al?nacak
}