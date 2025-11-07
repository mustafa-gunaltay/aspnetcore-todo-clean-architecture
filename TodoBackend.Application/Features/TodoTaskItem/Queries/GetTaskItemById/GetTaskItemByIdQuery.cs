using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemById;

public record GetTaskItemByIdQuery(
    int TaskItemId) : IRequest<Result<TaskItemViewModel>>;
