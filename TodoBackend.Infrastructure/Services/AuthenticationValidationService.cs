using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Infrastructure.Services;

/// <summary>
/// Implementation of authentication validation service
/// Infrastructure layer implementation of domain interface
/// </summary>
public class AuthenticationValidationService : IAuthenticationValidationService
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthenticationValidationService> _logger;

    public AuthenticationValidationService(ICurrentUser currentUser, ILogger<AuthenticationValidationService> logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public AuthenticationResult ValidateUserAuthentication(int requestingUserId, string operationName)
    {
        // Check if there's an authenticated user
        if (_currentUser.UserId == null)
        {
            _logger.LogWarning("{OperationName} failed - no authenticated user found", operationName);
            return AuthenticationResult.Failure("Authentication required");
        }

        // Check if the requesting user matches the authenticated user
        if (_currentUser.UserId != requestingUserId)
        {
            _logger.LogWarning("{OperationName} failed - user ID {RequestUserId} does not match authenticated user ID {AuthenticatedUserId}", 
                operationName, requestingUserId, _currentUser.UserId);
            return AuthenticationResult.Failure("You can only perform this operation for yourself");
        }

        _logger.LogDebug("Authentication validated for {OperationName} - user ID {UserId} ({UserEmail})", 
            operationName, requestingUserId, _currentUser.UserName);

        return AuthenticationResult.Success();
    }

    public int? GetCurrentUserId()
    {
        return _currentUser.UserId;
    }

    public string GetCurrentUserName()
    {
        return _currentUser.UserName;
    }
}