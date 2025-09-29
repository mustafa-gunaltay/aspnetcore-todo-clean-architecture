using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Actors.Messages;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Infrastructure.Actors;

/// <summary>
/// Email g�nderme i?lemlerinden sorumlu Actor
/// Mevcut EmailSenderService'i kullanarak actual email g�nderim i?lemini yapar
/// </summary>
public class EmailSenderActor : ReceiveActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailSenderActor> _logger;

    public EmailSenderActor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        using var scope = serviceProvider.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<EmailSenderActor>>();

        // Message handling patterns
        ReceiveAsync<SendEmailReminder>(HandleSendEmail);

        _logger.LogDebug("EmailSenderActor created: {ActorPath}", Self.Path);
    }

    /// <summary>
    /// Email g�nderme mesaj?n? i?ler
    /// Mevcut EmailSenderService'i kullan?r (Clean Architecture uyumu i�in)
    /// </summary>
    private async Task HandleSendEmail(SendEmailReminder message)
    {
        try
        {
            _logger.LogDebug("Processing email send request for {Email} with {TaskCount} tasks", 
                message.Email, message.TaskTitles.Count);

            using var scope = _serviceProvider.CreateScope();
            var emailSenderService = scope.ServiceProvider.GetRequiredService<IEmailSenderService>();

            // Mevcut EmailSenderService'i kullan (Clean Architecture prensibi)
            await emailSenderService.SendTaskReminderAsync(message.Email, message.TaskTitles);

            // Ba?ar? bildirimi g�nder
            Sender.Tell(new EmailSent(message.Email, true));

            _logger.LogInformation("Successfully sent email to {Email}", message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", message.Email);
            
            // Hata bildirimi g�nder
            Sender.Tell(new EmailSent(message.Email, false, ex.Message));
        }
    }

    /// <summary>
    /// Actor Props factory method
    /// Akka.NET DI integration i�in gerekli
    /// </summary>
    public static Props Props(IServiceProvider serviceProvider) =>
        Akka.Actor.Props.Create(() => new EmailSenderActor(serviceProvider));

    protected override void PreStart()
    {
        _logger.LogDebug("EmailSenderActor starting: {ActorPath}", Self.Path);
        base.PreStart();
    }

    protected override void PostStop()
    {
        _logger.LogDebug("EmailSenderActor stopped: {ActorPath}", Self.Path);
        base.PostStop();
    }
}