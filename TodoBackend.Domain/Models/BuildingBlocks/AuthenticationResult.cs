namespace TodoBackend.Domain.Models.BuildingBlocks;

/// <summary>
/// Domain layer authentication validation result
/// Simple result model to maintain Clean Architecture principles
/// </summary>
public class AuthenticationResult
{
    public bool IsSuccessful { get; private set; }
    public string ErrorMessage { get; private set; } = string.Empty;

    private AuthenticationResult(bool isSuccessful, string errorMessage = "")
    {
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
    }

    public static AuthenticationResult Success()
    {
        return new AuthenticationResult(true);
    }

    public static AuthenticationResult Failure(string errorMessage)
    {
        return new AuthenticationResult(false, errorMessage);
    }
}