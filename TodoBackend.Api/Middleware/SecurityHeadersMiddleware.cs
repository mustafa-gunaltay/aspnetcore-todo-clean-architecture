namespace TodoBackend.Api.Middleware;

/// <summary>
/// XSS ve di?er web güvenlik aç?klar?na kar?? HTTP header'lar?n? ayarlar
/// Clean Architecture: API katman?nda cross-cutting concern
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogDebug("Applying security headers for request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // XSS Protection - Browser'?n built-in XSS korumas?n? aktifle?tir
            context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
            
            // Content Type Options - MIME type sniffing'i önle
            context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            
            // Frame Options - Clickjacking korumas?
            context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
            
            // Referrer Policy - Referrer bilgisini kontrol et
            context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");

            // Content Security Policy - XSS ve data injection korumas?
            var cspPolicy = BuildContentSecurityPolicy(context);
            context.Response.Headers.TryAdd("Content-Security-Policy", cspPolicy);
            
            // Strict Transport Security (HTTPS)
            if (context.Request.IsHttps)
            {
                context.Response.Headers.TryAdd("Strict-Transport-Security", 
                    "max-age=31536000; includeSubDomains; preload");
            }

            // Permissions Policy - Browser feature'lar?n? k?s?tla
            context.Response.Headers.TryAdd("Permissions-Policy", 
                "camera=(), microphone=(), geolocation=(), payment=()");

            // X-Permitted-Cross-Domain-Policies - Cross-domain policy dosyalar?n? k?s?tla
            context.Response.Headers.TryAdd("X-Permitted-Cross-Domain-Policies", "none");

            _logger.LogDebug("Security headers applied successfully for request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying security headers for request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            // Hata olsa da request'i devam ettir
        }

        await _next(context);
    }

    /// <summary>
    /// Context'e göre dinamik Content Security Policy olu?turur
    /// </summary>
    private string BuildContentSecurityPolicy(HttpContext context)
    {
        var isDevelopment = context.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

        if (isDevelopment)
        {
            // Development ortam? için daha esnek CSP
            return "default-src 'self'; " +
                   "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdnjs.cloudflare.com; " +
                   "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com https://cdnjs.cloudflare.com; " +
                   "img-src 'self' data: https: blob:; " +
                   "font-src 'self' https://fonts.gstatic.com https://cdnjs.cloudflare.com; " +
                   "connect-src 'self' https: wss:; " +
                   "frame-ancestors 'none'; " +
                   "base-uri 'self'; " +
                   "form-action 'self';";
        }
        else
        {
            // Production ortam? için s?k? CSP
            return "default-src 'self'; " +
                   "script-src 'self'; " +
                   "style-src 'self' https://fonts.googleapis.com; " +
                   "img-src 'self' data: https:; " +
                   "font-src 'self' https://fonts.gstatic.com; " +
                   "connect-src 'self' https:; " +
                   "frame-ancestors 'none'; " +
                   "base-uri 'self'; " +
                   "form-action 'self'; " +
                   "upgrade-insecure-requests;";
        }
    }
}