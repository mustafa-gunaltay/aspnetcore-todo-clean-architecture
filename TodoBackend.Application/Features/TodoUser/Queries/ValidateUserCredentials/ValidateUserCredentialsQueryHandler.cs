using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials;

public class ValidateUserCredentialsQueryHandler : IRequestHandler<ValidateUserCredentialsQuery, Result<bool>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<ValidateUserCredentialsQueryHandler> _logger;

    public ValidateUserCredentialsQueryHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher, ILogger<ValidateUserCredentialsQueryHandler> logger)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(ValidateUserCredentialsQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting credential validation for email {Email}", request.Email);
        
        try
        {
            // Get user by email
            _logger.LogDebug("Fetching user by email {Email} from repository", request.Email);
            var user = await _uow.UserRepository.GetByEmailAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                stopwatch.Stop();
                _logger.LogWarning("Credential validation failed - user with email {Email} not found after {Duration}ms", 
                    request.Email, stopwatch.ElapsedMilliseconds);
                return Result<bool>.Success(false, "Invalid email or password");
            }

            _logger.LogDebug("Found user with email {Email} (ID: {UserId}), verifying password", 
                request.Email, user.Id);

            // Verify password using password hasher
            var isPasswordValid = _passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt);

            stopwatch.Stop();

            if (isPasswordValid)
            {
                _logger.LogInformation("Credential validation successful for email {Email} (User ID: {UserId}) in {Duration}ms", 
                    request.Email, user.Id, stopwatch.ElapsedMilliseconds);
                
                return Result<bool>.Success(true, "Credentials are valid");
            }
            else
            {
                _logger.LogWarning("Credential validation failed - invalid password for email {Email} (User ID: {UserId}) after {Duration}ms", 
                    request.Email, user.Id, stopwatch.ElapsedMilliseconds);
                
                return Result<bool>.Success(false, "Invalid email or password");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to validate credentials for email {Email} after {Duration}ms", 
                request.Email, stopwatch.ElapsedMilliseconds);
            return Result<bool>.Failure($"Failed to validate credentials: {ex.Message}");
        }
        finally
        {
            // Performance monitoring for authentication operations
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow credential validation detected: {Duration}ms for email {Email} (threshold: 500ms). Consider optimizing password hashing.", 
                    stopwatch.ElapsedMilliseconds, request.Email);
            }
        }
    }
}