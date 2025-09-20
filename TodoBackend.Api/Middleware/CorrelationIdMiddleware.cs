using Serilog.Context;

namespace TodoBackend.Api.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Request'ten correlation ID al veya yeni olu?tur
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Response header'?na ekle
        context.Response.Headers.Add(CorrelationIdHeaderName, correlationId);
        
        // HttpContext.Items'a ekle (controller'larda kullan?m için)
        context.Items["CorrelationId"] = correlationId;
        
        // Serilog LogContext'e ekle - tüm loglarda otomatik görünür
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogInformation("Request started {RequestMethod} {RequestPath} with correlation ID {CorrelationId}", 
                context.Request.Method, context.Request.Path, correlationId);
            
            try
            {
                await _next(context);
                
                _logger.LogInformation("Request completed {RequestMethod} {RequestPath} with status {StatusCode} - CorrelationId: {CorrelationId}", 
                    context.Request.Method, context.Request.Path, context.Response.StatusCode, correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed {RequestMethod} {RequestPath} - CorrelationId: {CorrelationId}", 
                    context.Request.Method, context.Request.Path, correlationId);
                throw;
            }
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Client'tan gelen correlation ID varsa kullan
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId) 
            && !string.IsNullOrEmpty(correlationId))
        {
            return correlationId.ToString();
        }

        // Yoksa yeni olu?tur
        return Guid.NewGuid().ToString("D");
    }
}