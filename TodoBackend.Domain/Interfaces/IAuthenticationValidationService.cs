using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Domain.Interfaces;

/// <summary>
/// Service for validating authentication and authorization in command handlers
/// Follows Clean Architecture principles by keeping interface in Domain layer
/// </summary>
public interface IAuthenticationValidationService
{
    /// <summary>
    /// Validates that the requesting user matches the current authenticated user
    /// </summary>
    /// <param name="requestingUserId">User ID from the command</param>
    /// <param name="operationName">Name of the operation for logging (e.g., "create category")</param>
    /// <returns>AuthenticationResult indicating success or failure with appropriate error message</returns>
    AuthenticationResult ValidateUserAuthentication(int requestingUserId, string operationName);

    /// <summary>
    /// Gets the current authenticated user's ID
    /// </summary>
    /// <returns>Current user ID if authenticated, null otherwise</returns>
    int? GetCurrentUserId();

    /// <summary>
    /// Gets the current authenticated user's email/username
    /// </summary>
    /// <returns>Current user's email if authenticated, "system" otherwise</returns>
    string GetCurrentUserName();
}