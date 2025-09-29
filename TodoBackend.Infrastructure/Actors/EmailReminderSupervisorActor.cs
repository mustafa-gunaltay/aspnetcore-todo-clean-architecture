using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Actors.Messages;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Infrastructure.Actors;

/// <summary>
/// Email Reminder i?lemlerinin ana koordinatörü (Supervisor Actor)
/// Tüm kullan?c?lar? getirir, her biri için UserProcessor actor'? olu?turur
/// Hata yönetimi ve monitoring yapar
/// </summary>
public class EmailReminderSupervisorActor : ReceiveActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailReminderSupervisorActor> _logger;

    // ??lem takibi için
    private readonly Dictionary<int, IActorRef> _userProcessors = new();
    private readonly Dictionary<int, DateTime> _processingStartTimes = new();
    private int _totalUsers;
    private int _completedUsers;
    private int _successCount;
    private int _failureCount;

    public EmailReminderSupervisorActor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        using var scope = serviceProvider.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailReminderSupervisorActor>>();

        // Message handling patterns
        ReceiveAsync<ProcessAllUsersReminders>(HandleProcessAllUsers);
        Receive<UserReminderCompleted>(HandleUserCompleted);
        Receive<GetActiveUserCount>(HandleGetActiveUserCount);
        Receive<GetProcessingStatus>(HandleGetProcessingStatus);
        Receive<AllUsersProcessingCompleted>(HandleAllUsersCompleted);

        _logger.LogInformation("EmailReminderSupervisorActor created: {ActorPath}", Self.Path);
    }

    /// <summary>
    /// Tüm kullan?c?lar için email reminder i?lemini ba?lat?r
    /// Mevcut EmailNotificationService'deki logic'in ayn?s?n? Actor Model ile yapar
    /// </summary>
    private async Task HandleProcessAllUsers(ProcessAllUsersReminders message)
    {
        try
        {
            _logger.LogInformation("Starting email reminder process for all users via Actor System");

            // State'i s?f?rla
            _userProcessors.Clear();
            _processingStartTimes.Clear();
            _totalUsers = 0;
            _completedUsers = 0;
            _successCount = 0;
            _failureCount = 0;

            using var scope = _serviceProvider.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            // Mevcut EmailNotificationService'deki ayn? logic
            var users = await userRepository.GetAllAsync();
            var activeUsers = users.Where(u => !u.IsDeleted).ToList();

            _totalUsers = activeUsers.Count;
            _logger.LogInformation("Processing {UserCount} active users for task reminders", _totalUsers);

            if (!activeUsers.Any())
            {
                _logger.LogInformation("No active users found, completing process");
                Context.Self.Tell(new AllUsersProcessingCompleted(0, 0));
                return;
            }

            // Her kullan?c? için ayr? UserProcessor actor'? olu?tur ve paralel i?le
            foreach (var user in activeUsers)
            {
                var userProcessorName = $"user-processor-{user.Id}";
                var userProcessor = Context.ActorOf(
                    UserProcessorActor.Props(_serviceProvider), 
                    userProcessorName);

                _userProcessors[user.Id] = userProcessor;
                _processingStartTimes[user.Id] = DateTime.UtcNow;

                // Mesaj gönder (paralel i?lem!)
                userProcessor.Tell(new ProcessUserReminder(user.Id, user.Email));

                _logger.LogDebug("Started processing user {UserId} ({Email}) with actor {ActorName}", 
                    user.Id, user.Email, userProcessorName);
            }

            _logger.LogInformation("Dispatched {UserCount} users to parallel processing", _totalUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start email reminder process");
            Context.Self.Tell(new AllUsersProcessingCompleted(0, _totalUsers));
        }
    }

    /// <summary>
    /// Tek bir kullan?c?n?n i?lemi tamamland? mesaj?n? i?ler
    /// Mevcut EmailNotificationService'deki success/failure counting logic'ini yapar
    /// </summary>
    private void HandleUserCompleted(UserReminderCompleted message)
    {
        _completedUsers++;

        // Processing time hesapla
        var processingTime = _processingStartTimes.TryGetValue(message.UserId, out var startTime)
            ? DateTime.UtcNow - startTime
            : TimeSpan.Zero;

        if (message.Success)
        {
            _successCount++;
            _logger.LogDebug("User {UserId} reminder completed successfully in {ProcessingTime:F2}s", 
                message.UserId, processingTime.TotalSeconds);
        }
        else
        {
            _failureCount++;
            _logger.LogError("User {UserId} reminder failed after {ProcessingTime:F2}s: {Error}", 
                message.UserId, processingTime.TotalSeconds, message.ErrorMessage);
        }

        // Actor'? temizle
        if (_userProcessors.TryGetValue(message.UserId, out var userProcessor))
        {
            userProcessor.Tell(PoisonPill.Instance);
            _userProcessors.Remove(message.UserId);
        }
        _processingStartTimes.Remove(message.UserId);

        // Tüm kullan?c?lar tamamland? m??
        if (_completedUsers >= _totalUsers)
        {
            _logger.LogInformation("All users processing completed. Success: {SuccessCount}, Failures: {FailureCount}", 
                _successCount, _failureCount);

            Context.Self.Tell(new AllUsersProcessingCompleted(_successCount, _failureCount));
        }
        else
        {
            _logger.LogDebug("Processing progress: {CompletedCount}/{TotalCount} users completed", 
                _completedUsers, _totalUsers);
        }
    }

    /// <summary>
    /// ??lem durumu sorgusunu i?ler (monitoring için)
    /// </summary>
    private void HandleGetProcessingStatus(GetProcessingStatus message)
    {
        var status = new
        {
            TotalUsers = _totalUsers,
            CompletedUsers = _completedUsers,
            SuccessCount = _successCount,
            FailureCount = _failureCount,
            ActiveProcessors = _userProcessors.Count
        };

        Sender.Tell(status);
    }

    /// <summary>
    /// Aktif kullan?c? say?s? sorgusunu i?ler (monitoring için)
    /// </summary>
    private void HandleGetActiveUserCount(GetActiveUserCount message)
    {
        Sender.Tell(_userProcessors.Count);
    }

    /// <summary>
    /// Tüm kullan?c?lar?n i?lemi tamamland???nda çal???r
    /// Mevcut EmailNotificationService'deki final logging'i yapar
    /// </summary>
    private void HandleAllUsersCompleted(AllUsersProcessingCompleted message)
    {
        _logger.LogInformation("Email reminder process completed via Actor System. Success: {SuccessCount}, Failures: {FailureCount}", 
            message.SuccessCount, message.FailureCount);

        // State'i temizle
        _userProcessors.Clear();
        _processingStartTimes.Clear();
        _totalUsers = 0;
        _completedUsers = 0;
        _successCount = 0;
        _failureCount = 0;
    }

    /// <summary>
    /// Actor Props factory method
    /// </summary>
    public static Props Props(IServiceProvider serviceProvider) =>
        Akka.Actor.Props.Create(() => new EmailReminderSupervisorActor(serviceProvider));

    /// <summary>
    /// Supervision Strategy - Child actor'lar fail olursa ne yapaca??m?z? belirler
    /// </summary>
    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(
            maxNrOfRetries: 3,
            withinTimeRange: TimeSpan.FromMinutes(1),
            localOnlyDecider: ex =>
            {
                switch (ex)
                {
                    case TimeoutException:
                        _logger.LogWarning("Actor timeout, resuming: {Exception}", ex.Message);
                        return Directive.Resume;
                    
                    case ArgumentException:
                        _logger.LogError("Actor argument exception, stopping: {Exception}", ex.Message);
                        return Directive.Stop;
                    
                    default:
                        _logger.LogError("Actor exception, restarting: {Exception}", ex.Message);
                        return Directive.Restart;
                }
            });
    }

    protected override void PreStart()
    {
        _logger.LogInformation("EmailReminderSupervisorActor starting: {ActorPath}", Self.Path);
        base.PreStart();
    }

    protected override void PostStop()
    {
        _logger.LogInformation("EmailReminderSupervisorActor stopped: {ActorPath}", Self.Path);
        
        // Tüm child actor'lar? temizle
        foreach (var processor in _userProcessors.Values)
        {
            processor.Tell(PoisonPill.Instance);
        }
        _userProcessors.Clear();
        
        base.PostStop();
    }
}