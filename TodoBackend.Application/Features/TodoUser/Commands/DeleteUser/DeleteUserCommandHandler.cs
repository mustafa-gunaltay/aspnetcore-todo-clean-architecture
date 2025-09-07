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

            // YENİ GEREKSINIM: User silindiğinde o user'ın task'ları da soft delete edilmeli
            // Bulk operation ile performance optimizasyonu
            var deletedTaskCount = await _uow.TaskItemRepository.SoftDeleteAllTasksByUserIdAsync(
                request.UserId, 
                cancellationToken);

            // Soft delete user
            await _uow.UserRepository.DeleteAsync(user, cancellationToken);
            
            // Save all changes in one transaction
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success($"User and {deletedTaskCount} associated tasks deleted successfully");
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