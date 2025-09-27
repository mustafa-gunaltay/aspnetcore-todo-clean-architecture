using Hangfire;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.BackgroundJobs;

namespace TodoBackend.Infrastructure.BackgroundJobs;

public class EmailReminderJob
{
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<EmailReminderJob> _logger;

    public EmailReminderJob(
        IEmailNotificationService emailNotificationService,
        ILogger<EmailReminderJob> logger)
    {
        _emailNotificationService = emailNotificationService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    [JobDisplayName("Send Task Reminders to All Users")]
    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting scheduled task reminder job");
        
        try
        {
            await _emailNotificationService.SendTaskRemindersToAllUsersAsync();
            _logger.LogInformation("Scheduled task reminder job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled task reminder job failed");
            throw; // Hangfire retry için
        }
    }
}