namespace TodoBackend.Domain.Interfaces.Inside;

public interface IEmailNotificationService
{
    Task SendTaskRemindersToAllUsersAsync();
}