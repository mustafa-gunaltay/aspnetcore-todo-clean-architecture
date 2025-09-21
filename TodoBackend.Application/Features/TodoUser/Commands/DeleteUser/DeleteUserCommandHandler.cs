using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoUser.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(ITodoBackendUnitOfWork uow, ILogger<DeleteUserCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting user deletion process for ID {UserId}", request.UserId);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User deletion failed - user with ID {UserId} not found", request.UserId);
                return Result.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging

            // YENİ GEREKSINIM: User silindiğinde o user'ın task'ları da soft delete edilmeli
            // Bulk operation ile performance optimizasyonu
            _logger.LogDebug("Deleting all tasks for user ID {UserId} ({UserEmail})", request.UserId, userEmail);
            var deletedTaskCount = await _uow.TaskItemRepository.SoftDeleteAllTasksByUserIdAsync(
                request.UserId, 
                cancellationToken);

            _logger.LogDebug("Deleted {TaskCount} tasks for user ID {UserId}", deletedTaskCount, request.UserId);

            // Soft delete user
            _logger.LogDebug("Performing delete for user ID {UserId} ({UserEmail})", request.UserId, userEmail);
            await _uow.UserRepository.DeleteAsync(user, cancellationToken);
            
            // Save all changes in one transaction
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("User ID {UserId} ({UserEmail}) and {TaskCount} associated tasks deleted successfully in {Duration}ms", 
                request.UserId, userEmail, deletedTaskCount, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 2000)
            {
                _logger.LogWarning("Slow user deletion detected: {Duration}ms for user ID {UserId} with {TaskCount} tasks (threshold: 2000ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId, deletedTaskCount);
            }

            return Result.Success($"User and {deletedTaskCount} associated tasks deleted successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during user deletion for ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.UserId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "User deletion failed with exception for ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to delete user: {ex.Message}");
        }
    }
}