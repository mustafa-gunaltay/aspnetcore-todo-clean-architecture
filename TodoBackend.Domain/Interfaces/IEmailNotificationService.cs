namespace TodoBackend.Application.BackgroundJobs;

public interface IEmailNotificationService
{
    Task SendTaskRemindersToAllUsersAsync();
}