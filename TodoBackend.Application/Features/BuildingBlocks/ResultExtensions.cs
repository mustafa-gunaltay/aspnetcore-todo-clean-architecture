using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Application.Features.BuildingBlocks;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this T value, string? successMessage = null)
    {
        return Result<T>.Success(value, successMessage);
    }

    public static Result ToResult(this bool success, string? successMessage = null, string? errorMessage = null)
    {
        return success
            ? Result.Success(successMessage)
            : Result.Failure(errorMessage ?? "Operation failed.");
    }

    public static async Task<Result<T>> ToResultAsync<T>(this Task<T> task, string? successMessage = null)
    {
        try
        {
            var value = await task;
            return Result<T>.Success(value, successMessage);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }

    public static Result<TResult> Map<T, TResult>(this Result<T> result, Func<T, TResult> mapper)
    {
        if (!result.IsSuccess || result.Value == null)
            return Result<TResult>.Failure(result.GetFirstError());

        try
        {
            var mappedValue = mapper(result.Value);
            return Result<TResult>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure($"Mapping failed: {ex.Message}");
        }
    }

    public static async Task<Result<TResult>> MapAsync<T, TResult>(this Result<T> result, Func<T, Task<TResult>> mapper)
    {
        if (!result.IsSuccess || result.Value == null)
            return Result<TResult>.Failure(result.GetFirstError());

        try
        {
            var mappedValue = await mapper(result.Value);
            return Result<TResult>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TResult>.Failure($"Async mapping failed: {ex.Message}");
        }
    }
}