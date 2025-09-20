using Serilog;
using TodoBackend.Api.Middleware;

namespace TodoBackend.Api.Configs;

public static class AppUseExtensions
{
    /// <summary>
    /// WebApplication'a middleware pipeline'ını yapılandırır
    /// </summary>
    public static IApplicationBuilder AppUse(this IApplicationBuilder app, IConfiguration configuration)
    {
        var webApp = (WebApplication)app;
        
        // Development-specific middleware
        if (webApp.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Base middleware
        app.UseHttpsRedirection();

        // Correlation ID middleware - En başta olmalı
        app.UseMiddleware<CorrelationIdMiddleware>();

        // Serilog HTTP request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
            };
        });

        // CORS - Authentication'dan önce olmalı
        app.UseCors();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Controllers - WebApplication üzerinden map et
        webApp.MapControllers();

        return app;
    }

    /// <summary>
    /// Serilog konfigürasyonunu yapılandırır - Static method olarak
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build())
            .CreateLogger();
    }

    /// <summary>
    /// WebApplicationBuilder'a Serilog'u entegre eder
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        return builder;
    }
}
