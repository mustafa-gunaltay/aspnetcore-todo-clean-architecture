using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Actors.Messages;

namespace TodoBackend.Infrastructure.Actors;

/// <summary>
/// Tek bir kullan?c?n?n email reminder i?lemini koordine eden Actor
/// TaskFetcher ve EmailSender actor'lar?n? yönetir
/// </summary>
public class UserProcessorActor : ReceiveActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserProcessorActor> _logger;
    
    // Child actor referanslar?
    private IActorRef? _taskFetcher;
    private IActorRef? _emailSender;
    
    // ??lem state'i
    private ProcessUserReminder? _currentUserMessage;
    private bool _isProcessing;

    public UserProcessorActor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        using var scope = serviceProvider.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<UserProcessorActor>>();

        // Message handling patterns
        Receive<ProcessUserReminder>(HandleProcessUser);
        Receive<TasksRetrieved>(HandleTasksRetrieved);
        Receive<EmailSent>(HandleEmailSent);

        _logger.LogDebug("UserProcessorActor created: {ActorPath}", Self.Path);
    }

    /// <summary>
    /// Kullan?c? i?leme mesaj?n? ba?lat?r
    /// 1. Önce task'lar? getirir
    /// 2. Task varsa email gönderir
    /// 3. Sonuç olarak parent'a bildirim gönderir
    /// </summary>
    private void HandleProcessUser(ProcessUserReminder message)
    {
        if (_isProcessing)
        {
            _logger.LogWarning("UserProcessor already processing user {UserId}, ignoring new request", 
                message.UserId);
            return;
        }

        _currentUserMessage = message;
        _isProcessing = true;

        _logger.LogDebug("Starting to process user {UserId} ({Email})", 
            message.UserId, message.Email);

        // Task fetcher actor'? olu?tur ve task'lar? getir
        _taskFetcher = Context.ActorOf(TaskFetcherActor.Props(_serviceProvider), "task-fetcher");
        _taskFetcher.Tell(new FetchUserTasks(message.UserId));
    }

    /// <summary>
    /// Task'lar getirildi mesaj?n? i?ler
    /// E?er task varsa email gönderme i?lemini ba?lat?r
    /// </summary>
    private void HandleTasksRetrieved(TasksRetrieved message)
    {
        if (_currentUserMessage == null)
        {
            _logger.LogWarning("Received TasksRetrieved but no current user message");
            return;
        }

        if (message.TaskTitles.Any())
        {
            _logger.LogDebug("User {UserId} has {TaskCount} tasks, sending email", 
                _currentUserMessage.UserId, message.TaskTitles.Count);

            // Email sender actor'? olu?tur ve email gönder
            _emailSender = Context.ActorOf(EmailSenderActor.Props(_serviceProvider), "email-sender");
            _emailSender.Tell(new SendEmailReminder(_currentUserMessage.Email, message.TaskTitles));
        }
        else
        {
            _logger.LogDebug("User {UserId} has no tasks, skipping email", 
                _currentUserMessage.UserId);

            // Task yok, i?lem ba?ar?yla tamamland?
            CompleteProcessing(true);
        }
    }

    /// <summary>
    /// Email gönderildi mesaj?n? i?ler
    /// ??lemi tamamlar ve parent'a bildirim gönderir
    /// </summary>
    private void HandleEmailSent(EmailSent message)
    {
        if (_currentUserMessage == null)
        {
            _logger.LogWarning("Received EmailSent but no current user message");
            return;
        }

        if (message.Success)
        {
            _logger.LogInformation("Successfully completed processing for user {UserId} ({Email})", 
                _currentUserMessage.UserId, _currentUserMessage.Email);
            CompleteProcessing(true);
        }
        else
        {
            _logger.LogError("Failed to send email for user {UserId} ({Email}): {Error}", 
                _currentUserMessage.UserId, _currentUserMessage.Email, message.ErrorMessage);
            CompleteProcessing(false, message.ErrorMessage);
        }
    }

    /// <summary>
    /// ??lemi tamamlar ve parent actor'a bildirim gönderir
    /// </summary>
    private void CompleteProcessing(bool success, string? errorMessage = null)
    {
        if (_currentUserMessage != null)
        {
            // Parent'a tamamland? bildirimi gönder
            Context.Parent.Tell(new UserReminderCompleted(_currentUserMessage.UserId, success, errorMessage));
        }

        // State'i temizle
        _currentUserMessage = null;
        _isProcessing = false;

        // Child actor'lar? temizle
        _taskFetcher?.Tell(PoisonPill.Instance);
        _emailSender?.Tell(PoisonPill.Instance);
        _taskFetcher = null;
        _emailSender = null;
    }

    /// <summary>
    /// Actor Props factory method
    /// </summary>
    public static Props Props(IServiceProvider serviceProvider) =>
        Akka.Actor.Props.Create(() => new UserProcessorActor(serviceProvider));

    protected override void PreStart()
    {
        _logger.LogDebug("UserProcessorActor starting: {ActorPath}", Self.Path);
        base.PreStart();
    }

    protected override void PostStop()
    {
        _logger.LogDebug("UserProcessorActor stopped: {ActorPath}", Self.Path);
        
        // Child actor'lar? temizle
        _taskFetcher?.Tell(PoisonPill.Instance);
        _emailSender?.Tell(PoisonPill.Instance);
        
        base.PostStop();
    }
}