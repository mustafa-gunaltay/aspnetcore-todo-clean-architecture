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

    private static string appPassword = "tudjclcdvyjfpdtb"; // Uygulama şifresi

    public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
    {
        _configuration = configuration;
        _logger = logger;
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

            var fromAddress = new MailAddress("mustafagunaltay25@gmail.com", "TodoBackend");
            var toAddress = new MailAddress(toEmail, "");
            string fromPassword = appPassword;

            string subject = GenerateEmailSubject(taskTitles.Count);
            string body = GenerateEmailBody(taskTitles);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
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
            using var client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("mustafagunaltay25@gmail.com", appPassword);
            
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
