using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoUser.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher, ILogger<UpdateUserCommandHandler> logger)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting user update process for ID {UserId}", request.UserId);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User update failed - user with ID {UserId} not found", request.UserId);
                return Result.Failure("User not found");
            }

            var oldEmail = user.Email; // Store for logging
            var changes = new List<string>();

            // Check if new email is unique (exclude current user)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogDebug("Checking if new email {Email} is unique for user ID {UserId}", request.Email, request.UserId);
                var isEmailUnique = await _uow.UserRepository.IsEmailUniqueAsync(request.Email, request.UserId, cancellationToken);
                if (!isEmailUnique)
                {
                    _logger.LogWarning("User update failed - duplicate email {Email} for user ID {UserId}", request.Email, request.UserId);
                    return Result.Failure("Email address is already in use");
                }

                // Update email using domain method
                user.ChangeEmail(request.Email);
                changes.Add($"Email: '{oldEmail}' -> '{request.Email}'");
            }

            // Update password only if provided using domain methods
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogDebug("Updating password for user ID {UserId}", request.UserId);
                // Hash the new password
                var (hash, salt) = _passwordHasher.Hash(request.Password);

                // Apply new password using domain method
                user.ApplyNewPassword(hash, salt);
                changes.Add("Password updated");
            }

            // Track changes
            _logger.LogDebug("Updating user ID {UserId} with changes: {Changes}", 
                request.UserId, string.Join(", ", changes));

            // Save changes
            await _uow.UserRepository.UpdateAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("User ID {UserId} updated successfully in {Duration}ms with changes: {Changes}", 
                request.UserId, stopwatch.ElapsedMilliseconds, string.Join(", ", changes));

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow user update detected: {Duration}ms for user ID {UserId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId);
            }

            return Result.Success("User updated successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during user update for ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.UserId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "User update failed with exception for ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to update user: {ex.Message}");
        }
    }
}