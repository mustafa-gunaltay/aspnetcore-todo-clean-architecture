using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;

public class CreateTaskItemCommandHandler : IRequestHandler<CreateTaskItemCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public CreateTaskItemCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<int>> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists and is active
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<int>.Failure("User not found");
            }

            // Create TaskItem using domain constructor
            var taskItem = new TaskItem(
                title: request.Title,
                description: request.Description,
                priority: request.Priority,
                dueDate: request.DueDate,
                userId: request.UserId
            );

            // Save TaskItem
            await _uow.TaskItemRepository.AddAsync(taskItem, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(taskItem.Id, "Task created successfully");
        }
        catch (DomainException dex)
        {
            return Result<int>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to create task: {ex.Message}");
        }
    }
}