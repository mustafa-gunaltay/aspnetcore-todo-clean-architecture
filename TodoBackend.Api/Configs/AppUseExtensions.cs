using Serilog;
using Serilog.Events;
using Serilog.Filters;
using TodoBackend.Api.Middleware;
using Hangfire;
using Hangfire.Dashboard;
using TodoBackend.Infrastructure.BackgroundJobs;

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

        // Serilog HTTP request logging with enhanced enrichment
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
            
            // Performance logging için threshold
            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (elapsed > 1000)
                    return LogEventLevel.Warning; // Slow requests
                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error; // Server errors
                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning; // Client errors
                
                return LogEventLevel.Information; // Normal requests
            };
        });

        // CORS - Authentication'dan önce olmalı
        app.UseCors();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        // Hangfire Dashboard - Authentication'dan sonra olmalı
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = new[] { new HangfireAuthorizationFilter() }
        });

        // Hangfire recurring jobs'ı başlat
        SetupRecurringJobs();

        // Controllers - WebApplication üzerinden map et
        webApp.MapControllers();

        return app;
    }

    /// <summary>
    /// Recurring job'ları ayarla
    /// </summary>
    private static void SetupRecurringJobs()
    {
        // Her 1 dakikada bir task reminder gönder
        RecurringJob.AddOrUpdate<EmailReminderJob>(
            "task-reminders",
            job => job.ExecuteAsync(),
            "*/1 * * * *"); // Cron expression: Her 1 dakika

        Log.Information("Hangfire recurring jobs configured: Task reminders every 1 minutes");
    }

    /// <summary>
    /// Hierarchical Serilog konfigürasyonunu yapılandırır - Her katman ayrı dizine log yazar
    /// </summary>
    public static void ConfigureSerilog()
    {
        var baseConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Log.Logger = new LoggerConfiguration()
            // Base configuration from appsettings.json for console
            .ReadFrom.Configuration(baseConfiguration)
            
            // Enrich with additional properties
            .Enrich.WithProperty("Application", "TodoBackend")
            .Enrich.WithProperty("Version", "1.0.0")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production")
            
            // APPLICATION LAYER LOGS - logs/application/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Application").Invoke(evt))
                .WriteTo.File(
                    path: "logs/application/application-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [APP] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // APPLICATION LAYER - Error logs
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Application").Invoke(evt) && 
                    evt.Level >= LogEventLevel.Error)
                .WriteTo.File(
                    path: "logs/application/application-errors-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 90,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [APP-ERR] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // APPLICATION LAYER - Performance logs
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Application").Invoke(evt) && 
                    (evt.MessageTemplate.Text.Contains("Slow") || 
                     evt.MessageTemplate.Text.Contains("PERFORMANCE") ||
                     evt.Properties.ContainsKey("Duration")))
                .WriteTo.File(
                    path: "logs/application/application-performance-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [APP-PERF] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // INFRASTRUCTURE LAYER LOGS - logs/infrastructure/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Infrastructure").Invoke(evt))
                .WriteTo.File(
                    path: "logs/infrastructure/infrastructure-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [INFRA] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // DATABASE LOGS - logs/infrastructure/database/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("Microsoft.EntityFrameworkCore").Invoke(evt) ||
                    evt.MessageTemplate.Text.Contains("DATABASE") ||
                    evt.MessageTemplate.Text.Contains("SQL"))
                .WriteTo.File(
                    path: "logs/infrastructure/database/database-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [DB] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // HANGFIRE LOGS - logs/infrastructure/hangfire/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("Hangfire").Invoke(evt) ||
                    Matching.FromSource("TodoBackend.Infrastructure.BackgroundJobs").Invoke(evt) ||
                    evt.MessageTemplate.Text.Contains("HANGFIRE") ||
                    evt.MessageTemplate.Text.Contains("Background Job"))
                .WriteTo.File(
                    path: "logs/infrastructure/hangfire/hangfire-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [HANGFIRE] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // API LAYER LOGS - logs/api/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Api").Invoke(evt))
                .WriteTo.File(
                    path: "logs/api/api-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [API] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // DOMAIN LAYER LOGS - logs/domain/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Domain").Invoke(evt))
                .WriteTo.File(
                    path: "logs/domain/domain-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 30,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [DOMAIN] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // SECURITY LOGS - logs/security/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("TodoBackend.Application.Features.Authentication").Invoke(evt) ||
                    Matching.FromSource("TodoBackend.Application.Features.TodoUser.Queries.ValidateUserCredentials").Invoke(evt) ||
                    Matching.FromSource("Microsoft.AspNetCore.Authentication").Invoke(evt) ||
                    Matching.FromSource("Microsoft.AspNetCore.Authorization").Invoke(evt) ||
                    evt.MessageTemplate.Text.Contains("SECURITY") ||
                    evt.MessageTemplate.Text.Contains("AUTH"))
                .WriteTo.File(
                    path: "logs/security/security-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 90,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [SEC] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // HTTP LOGS - logs/http/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => 
                    Matching.FromSource("Microsoft.AspNetCore").Invoke(evt) ||
                    evt.MessageTemplate.Text.Contains("HTTP"))
                .WriteTo.File(
                    path: "logs/http/http-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 15,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [HTTP] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // ALL ERRORS - logs/errors/
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(evt => evt.Level >= LogEventLevel.Error)
                .WriteTo.File(
                    path: "logs/errors/all-errors-.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true,
                    fileSizeLimitBytes: 10485760,
                    retainedFileCountLimit: 90,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [ERROR] [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}"))
            
            // MAIN LOG FILE - All logs combined
            .WriteTo.File(
                path: "logs/todobackend-.log",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 10485760,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
            
            .CreateLogger();

        // Log yapısının oluşturulacağını belirt
        Log.Information("Starting TodoBackend API with hierarchical logging structure");
        Log.Information("Log directories: application/, infrastructure/, api/, domain/, security/, http/, errors/, hangfire/");
        Log.Information("Each layer will log to its own directory for better organization and debugging");
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

/// <summary>
/// Hangfire Dashboard için basit authorization filter
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Development ortamında herkese izin ver
        // Production'da daha güvenli authentication eklenebilir
        return true;
    }
}
