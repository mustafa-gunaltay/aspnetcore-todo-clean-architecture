using Hangfire;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Actors;

namespace TodoBackend.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire + Akka.NET hibrit yakla??m?
/// Hangfire scheduling yapar, Akka.NET execution yapar
/// </summary>
public class EmailReminderJob
{
    private readonly IActorEmailNotificationService _actorEmailService;
    private readonly ILogger<EmailReminderJob> _logger;

    public EmailReminderJob(
        IActorEmailNotificationService actorEmailService,
        ILogger<EmailReminderJob> logger)
    {
        _actorEmailService = actorEmailService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 300)] // Ayn? anda sadece 1 tane çal??s?n
    [JobDisplayName("Send Task Reminders via Akka.NET Actor System")]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("=== Starting Hangfire Email Reminder Job ===");

        try
        {
            _logger.LogDebug("Checking ActorEmailNotificationService availability...");

            if (_actorEmailService == null)
            {
                _logger.LogError("ActorEmailNotificationService is null!");
                throw new InvalidOperationException("ActorEmailNotificationService is not available");
            }

            _logger.LogDebug("ActorEmailNotificationService is available, starting process...");

            // Actor sistemi üzerinden email reminder i?lemini ba?lat
            await _actorEmailService.StartEmailReminderProcessAsync();

            _logger.LogInformation("=== Successfully dispatched email reminder process to Akka.NET Actor System ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== FAILED: Email reminder process failed with exception ===");
            _logger.LogError("Exception Type: {ExceptionType}", ex.GetType().Name);
            _logger.LogError("Exception Message: {ExceptionMessage}", ex.Message);

            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerException}", ex.InnerException.Message);
            }

            throw; // Hangfire retry için
        }
    }
}