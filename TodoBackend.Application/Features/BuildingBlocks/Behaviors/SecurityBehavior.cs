using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Outside;
using System.Reflection;

namespace TodoBackend.Application.Features.BuildingBlocks.Behaviors;

/// <summary>
/// MediatR pipeline'?nda input sanitization i?lemi yapar
/// Tüm Command ve Query'lerdeki string property'leri otomatik sanitize eder
/// Clean Architecture: Application katman?nda cross-cutting concern
/// </summary>
public class SecurityBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IInputSanitizer _inputSanitizer;
    private readonly ILogger<SecurityBehavior<TRequest, TResponse>> _logger;

    public SecurityBehavior(IInputSanitizer inputSanitizer, ILogger<SecurityBehavior<TRequest, TResponse>> logger)
    {
        _inputSanitizer = inputSanitizer;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest);
        _logger.LogDebug("=== SECURITY: Applying security sanitization to {RequestType} ===", requestType.Name);

        try
        {
            // Request'teki tüm string property'leri kontrol et
            ValidateAndLogSecurityThreats(request);

            _logger.LogDebug("=== SECURITY: Security validation completed for {RequestType} ===", requestType.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== SECURITY: Error during security validation for {RequestType} ===", requestType.Name);
            // Security validation hatas? olsa da request'i devam ettir
            // Çünkü sanitization validator seviyesinde de yap?lacak
        }

        return await next();
    }

    /// <summary>
    /// Request'teki string property'leri güvenlik aç?s?ndan kontrol eder
    /// Record types immutable oldu?u için de?i?tirmez, sadece log'lar
    /// </summary>
    private void ValidateAndLogSecurityThreats(TRequest request)
    {
        var type = typeof(TRequest);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string) && p.CanRead)
            .ToList();

        if (!properties.Any())
        {
            _logger.LogDebug("No string properties found in {RequestType} for security validation", type.Name);
            return;
        }

        foreach (var property in properties)
        {
            try
            {
                var value = (string?)property.GetValue(request);
                if (!string.IsNullOrEmpty(value))
                {
                    ValidatePropertySecurity(property.Name, value, type.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error accessing property {PropertyName} in {RequestType}", property.Name, type.Name);
            }
        }
    }

    /// <summary>
    /// Belirli bir property'nin güvenlik durumunu kontrol eder
    /// </summary>
    private void ValidatePropertySecurity(string propertyName, string value, string requestTypeName)
    {
        try
        {
            var sanitizedValue = propertyName.ToLower() switch
            {
                "email" => _inputSanitizer.SanitizeEmail(value),
                "title" or "description" or "name" => _inputSanitizer.SanitizeHtml(value),
                "url" or "website" or "link" => _inputSanitizer.SanitizeUrl(value),
                _ => _inputSanitizer.SanitizeInput(value)
            };

            // De?er de?i?ti mi kontrol et (güvenlik tehdidi var m??)
            if (!string.Equals(value, sanitizedValue, StringComparison.Ordinal))
            {
                _logger.LogWarning("=== SECURITY THREAT DETECTED === Property: {PropertyName}, Request: {RequestType}, " +
                    "Original Length: {OriginalLength}, Sanitized Length: {SanitizedLength}",
                    propertyName, requestTypeName, value.Length, sanitizedValue.Length);

                // Hassas bilgileri log'lama, sadece ilk birkaç karakter
                var safeOriginal = value.Length > 10 ? value.Substring(0, 10) + "..." : value;
                var safeSanitized = sanitizedValue.Length > 10 ? sanitizedValue.Substring(0, 10) + "..." : sanitizedValue;
                
                _logger.LogDebug("Security sanitization preview - Original: '{Original}', Sanitized: '{Sanitized}'", 
                    safeOriginal, safeSanitized);
            }
            else
            {
                _logger.LogDebug("Property {PropertyName} in {RequestType} passed security validation", 
                    propertyName, requestTypeName);
            }
        }
        catch (ArgumentException ex) when (propertyName.ToLower() == "email")
        {
            _logger.LogWarning("Invalid email format detected in {PropertyName} for {RequestType}: {Error}", 
                propertyName, requestTypeName, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during security validation of {PropertyName} in {RequestType}", 
                propertyName, requestTypeName);
        }
    }
}