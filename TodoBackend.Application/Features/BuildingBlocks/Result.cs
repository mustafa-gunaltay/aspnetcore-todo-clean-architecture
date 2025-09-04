using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.BuildingBlocks;

// 1. Generic Result Class
public class Result<T> : Result
{
    public T? Value { get; private set; }

    public void AddValue(T value)
    {
        Value = value;
    }

    public bool HasValue => Value != null;

    // Implicit conversion operators for cleaner usage
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator T?(Result<T> result) => result.Value;

    // Factory methods specifically for generic result
    public static new Result<T> Success(T value, string? message = null)
    {
        var result = new Result<T> { IsSuccess = true };
        result.AddValue(value);
        if (!string.IsNullOrEmpty(message))
            result.AddSuccessMessage(message);
        return result;
    }

    public static new Result<T> Failure(string message)
    {
        var result = new Result<T> { IsSuccess = false };
        if (!string.IsNullOrEmpty(message))
            result.AddErrorMessage(message);
        return result;
    }

    // ✅ Validation failure için generic method
    public static new Result<T> ValidationFailure(Dictionary<string, string[]> validationErrors, string? message = null)
    {
        var result = new Result<T> { IsSuccess = false };
        result.AddValidationErrorMessages(validationErrors);
        if (!string.IsNullOrEmpty(message))
            result.AddErrorMessage(message);
        return result;
    }
}

// 2. Base Result Class
public class Result
{
    #region Properties
    public bool IsSuccess { get; protected set; }
    public List<string> Errors { get; protected set; } = [];
    public Dictionary<string, string[]> ValidationErrors { get; protected set; } = [];
    public List<string> Successes { get; protected set; } = [];
    #endregion

    #region Factory Methods
    public static Result Success(string? message = null)
    {
        var result = new Result { IsSuccess = true };
        if (!string.IsNullOrEmpty(message))
            result.AddSuccessMessage(message);
        return result;
    }

    public static Result<T> Success<T>(T value, string? message = null)
    {
        var result = new Result<T> { IsSuccess = true };
        result.AddValue(value);
        if (!string.IsNullOrEmpty(message))
            result.AddSuccessMessage(message);
        return result;
    }

    public static Result Failure(string message)
    {
        var result = new Result { IsSuccess = false };
        result.AddErrorMessage(message);
        return result;
    }

    public static Result<T> Failure<T>(string message)
    {
        var result = new Result<T> { IsSuccess = false };
        result.AddErrorMessage(message);
        return result;
    }

    public static Result ValidationFailure(Dictionary<string, string[]> validationErrors, string? message = null)
    {
        var result = new Result { IsSuccess = false };
        result.AddValidationErrorMessages(validationErrors);
        if (!string.IsNullOrEmpty(message))
            result.AddErrorMessage(message);
        return result;
    }

    public static Result<T> ValidationFailure<T>(Dictionary<string, string[]> validationErrors, string? message = null)
    {
        var result = new Result<T> { IsSuccess = false };
        result.AddValidationErrorMessages(validationErrors);
        if (!string.IsNullOrEmpty(message))
            result.AddErrorMessage(message);
        return result;
    }
    #endregion

    #region Success Methods
    public void OK()
    {
        Succeed("Operation completed successfully.");
    }

    public void Created()
    {
        Succeed("Resource created successfully.");
    }

    public void Updated()
    {
        Succeed("Resource updated successfully.");
    }

    public void Deleted()
    {
        Succeed("Resource deleted successfully.");
    }

    public void Redirect()
    {
        Succeed("Request redirected successfully.");
    }
    #endregion

    #region Error Methods
    public void BadRequest(string message, Dictionary<string, string[]>? validationErrors = null)
    {
        Failed(message);
        if (validationErrors != null)
            AddValidationErrorMessages(validationErrors);
    }

    public void Unauthorized(string? message = null)
    {
        Failed(message ?? "Unauthorized access.");
    }

    public void Forbidden(string? message = null)
    {
        Failed(message ?? "Access forbidden.");
    }

    public void NotFound(string? message = null)
    {
        Failed(message ?? "Resource not found.");
    }

    public void Conflict(string? message = null)
    {
        Failed(message ?? "Resource conflict occurred.");
    }

    public void InternalServerError(string? message = null)
    {
        Failed(message ?? "Internal server error occurred.");
    }
    #endregion

    #region Message Management
    public void AddSuccessMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message) && !Successes.Contains(message))
        {
            Successes.Add(message);
        }
    }

    public void AddErrorMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(message) && !Errors.Contains(message))
        {
            Errors.Add(message);
        }
    }

    public void AddValidationErrorMessages(Dictionary<string, string[]> errors)
    {
        foreach (var error in errors)
        {
            if (ValidationErrors.ContainsKey(error.Key))
            {
                // Merge arrays if key already exists
                var existingErrors = ValidationErrors[error.Key];
                ValidationErrors[error.Key] = existingErrors.Union(error.Value).ToArray();
            }
            else
            {
                ValidationErrors[error.Key] = error.Value;
            }
        }
    }
    #endregion

    #region Helper Methods
    private void Succeed(string message)
    {
        AddSuccessMessage(message);
        IsSuccess = true;
    }

    private void Failed(string message)
    {
        AddErrorMessage(message);
        IsSuccess = false;
    }

    public bool HasValidationErrors => ValidationErrors.Any();
    public bool HasErrors => Errors.Any() || HasValidationErrors;
    public string GetFirstError() => Errors.FirstOrDefault() ?? "Unknown error occurred.";
    public string GetAllErrorsAsString() => string.Join("; ", Errors);
    #endregion
}




