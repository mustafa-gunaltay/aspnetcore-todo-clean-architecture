using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;

namespace TodoBackend.Application.Features.BuildingBlocks.Behaviors;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();

            if (failures.Any())
            {
                // Validation errors'ı Dictionary<string, string[]> formatına çevir
                var validationErrors = failures
                    .GroupBy(f => f.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(f => f.ErrorMessage).ToArray()
                    );

                // Result<T> dönüyorsa ValidationFailure döndür
                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var validationFailureMethod = typeof(Result).GetMethod("ValidationFailure", 1, new[] { typeof(Dictionary<string, string[]>), typeof(string) });
                    var genericMethod = validationFailureMethod!.MakeGenericMethod(resultType);
                    var result = genericMethod.Invoke(null, new object[] { validationErrors, "Validation failed" })!;
                    return (TResponse)result;
                }
                // Normal Result dönüyorsa
                else if (typeof(TResponse) == typeof(Result))
                {
                    return (TResponse)(object)Result.ValidationFailure(validationErrors, "Validation failed");
                }
                // Diğer durumlar için exception fırlat
                else
                {
                    throw new ValidationException(failures);
                }
            }
        }

        return await next();
    }
}
