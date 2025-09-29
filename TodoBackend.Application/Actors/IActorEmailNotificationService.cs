using Akka.Actor;

namespace TodoBackend.Application.Actors;

/// <summary>
/// Actor-based Email Notification Service Interface
/// Clean Architecture: Application katman?nda tan?mlan?r ��nk� use case'e aittir
/// </summary>
public interface IActorEmailNotificationService
{
    /// <summary>
    /// Email Supervisor Actor referans?n? d�nd�r�r
    /// </summary>
    Task<IActorRef> GetEmailSupervisorAsync();
    
    /// <summary>
    /// Actor sistemi �zerinden email reminder i?lemini ba?lat?r
    /// Hangfire Job bu metodu �a??racak
    /// </summary>
    Task StartEmailReminderProcessAsync();
    
    /// <summary>
    /// Aktif i?lemlerin say?s?n? d�nd�r�r (monitoring i�in)
    /// </summary>
    Task<int> GetActiveProcessCountAsync();
}