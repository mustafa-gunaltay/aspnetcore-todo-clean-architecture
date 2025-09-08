using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetFilteredTaskItems;

public record GetFilteredTaskItemsQuery(
    int UserId,
    bool? IsCompleted = null,
    Priority? Priority = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int? CategoryId = null) : IRequest<Result<IReadOnlyList<TaskItemViewModel>>>;