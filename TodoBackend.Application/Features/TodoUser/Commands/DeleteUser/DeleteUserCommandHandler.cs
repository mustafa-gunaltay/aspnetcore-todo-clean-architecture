using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoUser.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public DeleteUserCommandHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure("User not found");
            }

            // Check if user has active tasks
            var userTasks = await _uow.TaskItemRepository.GetTasksByUserIdAsync(request.UserId, cancellationToken);
            if (userTasks.Any(t => !t.IsDeleted))
            {
                return Result.Failure("Cannot delete user with active tasks. Please delete or reassign tasks first.");
            }

            // Soft delete user
            await _uow.UserRepository.DeleteAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("User deleted successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}