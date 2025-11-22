using MediatR;
using System;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Enums.BuildingBlocks;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByFilter;

public record GetTaskItemsByFilterQuery(
    int UserId,
    bool? IsCompleted = null,
    Priority? Priority = null,
    DateTime? StartDueDate = null,
    DateTime? EndDueDate = null,
    OrderTaskItemByFilter? OrderBy = null,
    int PageSize = 10,
    int PageNumber = 1,
    OrderType OrderType = OrderType.Ascending
) : IRequest<Result<PagedList<TaskItemViewModel>>>;


public enum OrderTaskItemByFilter
{
    Title,
    Description,
    Priority,
    DueDate,
    CompletedAt,
    CreatedAt,
    UpdatedAt,
    DeletedAt
}
