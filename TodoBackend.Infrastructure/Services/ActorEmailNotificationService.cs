using Akka.Actor;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Actors;
using TodoBackend.Domain.Actors.Messages;
using TodoBackend.Infrastructure.Actors;

namespace TodoBackend.Infrastructure.Services;

/// <summary>
/// Actor-based Email Notification Service Implementation
/// Clean Architecture: Infrastructure katman?nda concrete implementation
/// </summary>
public class ActorEmailNotificationService : IActorEmailNotificationService
{
    private readonly ActorSystem _actorSystem;
    private readonly ILogger<ActorEmailNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IActorRef? _emailSupervisor;

    public ActorEmailNotificationService(
        ActorSystem actorSystem, 
        ILogger<ActorEmailNotificationService> logger,
        IServiceProvider serviceProvider)
    {
        _actorSystem = actorSystem;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Email Supervisor Actor referans?n? döndürür
    /// Lazy initialization pattern kullan?r
    /// </summary>
    public async Task<IActorRef> GetEmailSupervisorAsync()
    {
        try
        {
            _logger.LogDebug("Getting EmailSupervisor actor reference...");
            
            if (_emailSupervisor == null)
            {
                // Mevcut actor'? kontrol et
                var actorPath = "/user/email-reminder-supervisor";
                var existingActor = await _actorSystem.ActorSelection(actorPath).ResolveOne(TimeSpan.FromSeconds(1))
                    .ConfigureAwait(false);
                
                if (existingActor != null)
                {
                    _logger.LogInformation("Using existing EmailReminderSupervisor actor: {ActorPath}", existingActor.Path);
                    _emailSupervisor = existingActor;
                    return _emailSupervisor;
                }
            }
            else
            {
                // Actor hala alive m? kontrol et
                var isAlive = await _emailSupervisor.Ask<bool>(new GetProcessingStatus(), TimeSpan.FromSeconds(2))
                    .ContinueWith(t => !t.IsFaulted, TaskContinuationOptions.ExecuteSynchronously);
                
                if (isAlive)
                {
                    _logger.LogDebug("Using existing alive EmailSupervisor actor: {ActorPath}", _emailSupervisor.Path);
                    return _emailSupervisor;
                }
                else
                {
                    _logger.LogWarning("EmailSupervisor actor is dead, will create new one");
                    _emailSupervisor = null;
                }
            }

            // Yeni actor yarat - unique name ile
            var actorName = $"email-reminder-supervisor-{DateTime.UtcNow.Ticks}";
            
            _logger.LogDebug("Creating new EmailSupervisor actor with name: {ActorName}", actorName);
            
            if (_actorSystem == null)
            {
                throw new InvalidOperationException("ActorSystem is not available");
            }

            _emailSupervisor = _actorSystem.ActorOf(
                EmailReminderSupervisorActor.Props(_serviceProvider),
                actorName);

            _logger.LogInformation("EmailReminderSupervisor actor created successfully: {ActorPath}", 
                _emailSupervisor.Path);

            return _emailSupervisor;
        }
        catch (ActorNotFoundException)
        {
            // Actor bulunamad?, yeni yarataca??z
            _logger.LogDebug("EmailSupervisor actor not found, creating new one...");
            
            var actorName = $"email-reminder-supervisor-{DateTime.UtcNow.Ticks}";
            
            _emailSupervisor = _actorSystem.ActorOf(
                EmailReminderSupervisorActor.Props(_serviceProvider),
                actorName);

            _logger.LogInformation("EmailReminderSupervisor actor created successfully: {ActorPath}", 
                _emailSupervisor.Path);

            return _emailSupervisor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create or get EmailSupervisor actor");
            throw;
        }
    }

    /// <summary>
    /// Actor sistemi üzerinden email reminder i?lemini ba?lat?r
    /// Mevcut EmailNotificationService.SendTaskRemindersToAllUsersAsync() metodunun
    /// Actor-based kar??l???d?r
    /// </summary>
    public async Task StartEmailReminderProcessAsync()
    {
        try
        {
            _logger.LogInformation("=== Starting email reminder process via Actor System ===");
            _logger.LogDebug("ServiceProvider: {ServiceProviderType}", _serviceProvider?.GetType().Name ?? "NULL");

            var supervisor = await GetEmailSupervisorAsync();
            
            _logger.LogDebug("Sending ProcessAllUsersReminders message to supervisor...");
            supervisor.Tell(new ProcessAllUsersReminders());

            _logger.LogInformation("=== Email reminder process dispatched to Actor System successfully ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== FAILED: Failed to start email reminder process via Actor System ===");
            throw;
        }
    }

    /// <summary>
    /// Aktif i?lemlerin say?s?n? döndürür (monitoring için)
    /// </summary>
    public async Task<int> GetActiveProcessCountAsync()
    {
        try
        {
            var supervisor = await GetEmailSupervisorAsync();
            
            // Ask pattern kullanarak supervisor'dan bilgi al
            var result = await supervisor.Ask<int>(new GetActiveUserCount(), TimeSpan.FromSeconds(5));
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get active process count");
            return 0;
        }
    }
}