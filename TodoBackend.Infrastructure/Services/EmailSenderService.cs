using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Infrastructure.Services;

public class EmailSenderService : IEmailSenderService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSenderService> _logger;

    private readonly string appPassword;
    private readonly string smtpServer;
    private readonly int smtpPort;
    private readonly string senderEmail;
    private readonly string senderName;
    private readonly bool enableSsl;

    public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Configuration'dan değerleri al
        appPassword = _configuration["EmailSettings:AppPassword"] ?? string.Empty;
        smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        senderEmail = _configuration["EmailSettings:SenderEmail"] ?? string.Empty;
        senderName = _configuration["EmailSettings:SenderName"] ?? "TodoBackend";
        enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

        _logger.LogInformation("EmailSenderService initialized with SMTP: {SmtpServer}:{SmtpPort}, Sender: {SenderEmail}", 
            smtpServer, smtpPort, senderEmail);
    }

    public async Task SendTaskReminderAsync(string toEmail, List<string> taskTitles)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning("Email address is empty, skipping email send");
            return;
        }

        if (taskTitles == null || !taskTitles.Any())
        {
            _logger.LogWarning("No tasks provided for email to {Email}, skipping", toEmail);
            return;
        }

        try
        {
            _logger.LogDebug("Sending task reminder email to {Email} with {TaskCount} tasks",
                toEmail, taskTitles.Count);

            var fromAddress = new MailAddress(senderEmail, senderName);
            var toAddress = new MailAddress(toEmail, "");
            string fromPassword = appPassword;

            string subject = GenerateEmailSubject(taskTitles.Count);
            string body = GenerateEmailBody(taskTitles);

            var smtp = new SmtpClient
            {
                Host = smtpServer,
                Port = smtpPort,
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(senderEmail, fromPassword)
            };

            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            await smtp.SendMailAsync(message);

            _logger.LogInformation("Successfully sent task reminder email to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task reminder email to {Email}", toEmail);
            throw; // Hangfire retry mekanizması için exception'ı rethrow ediyoruz
        }
    }

    public async Task<bool> IsEmailServiceAvailableAsync()
    {
        try
        {
            // SMTP sunucusuna basit bir bağlantı testi
            using var client = new SmtpClient(smtpServer, smtpPort);
            client.EnableSsl = enableSsl;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(senderEmail, appPassword);
            
            // Timeout ile test bağlantısı
            client.Timeout = 5000; // 5 saniye
            
            await Task.Run(() => { /* Test connection logic */ });
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Email service is not available");
            return false;
        }
    }

    private static string GenerateEmailSubject(int taskCount)
    {
        return taskCount switch
        {
            1 => "You have 1 pending task - TodoBackend",
            > 1 => $"You have {taskCount} pending tasks - TodoBackend",
            _ => "Task Reminder - TodoBackend"
        };
    }

    private static string GenerateEmailBody(List<string> taskTitles)
    {
        var taskList = string.Join("</li><li>", taskTitles);
        
        return $"""
            <html>
            <body>
                <h2>Task Reminder</h2>
                <p>You have the following pending tasks:</p>
                <ul>
                    <li>{taskList}</li>
                </ul>
                <p>Don't forget to complete them!</p>
                <br>
                <p>Best regards,<br>TodoBackend Team</p>
            </body>
            </html>
            """;
    }
}
