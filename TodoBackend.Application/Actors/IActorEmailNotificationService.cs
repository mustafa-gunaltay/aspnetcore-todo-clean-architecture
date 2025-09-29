using Akka.Actor;

namespace TodoBackend.Application.Actors;

/// <summary>
/// Actor-based Email Notification Service Interface
/// Clean Architecture: Application katman?nda tan?mlan?r çünkü use case'e aittir
/// </summary>
public interface IActorEmailNotificationService
{
    /// <summary>
    /// Email Supervisor Actor referans?n? döndürür
    /// </summary>
    Task<IActorRef> GetEmailSupervisorAsync();
    
    /// <summary>
    /// Actor sistemi üzerinden email reminder i?lemini ba?lat?r
    /// Hangfire Job bu metodu ça??racak
    /// </summary>
    Task StartEmailReminderProcessAsync();
    
    /// <summary>
    /// Aktif i?lemlerin say?s?n? döndürür (monitoring için)
    /// </summary>
    Task<int> GetActiveProcessCountAsync();
}