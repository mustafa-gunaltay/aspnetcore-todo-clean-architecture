using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.BackgroundJobs;

public class EmailNotificationService : IEmailNotificationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITaskItemRepository _taskItemRepository;
    private readonly IEmailSenderService _emailService;
    private readonly ILogger<EmailNotificationService> _logger;

    public EmailNotificationService(
        IUserRepository userRepository,
        ITaskItemRepository taskItemRepository,
        IEmailSenderService emailService,
        ILogger<EmailNotificationService> logger)
    {
        _userRepository = userRepository;
        _taskItemRepository = taskItemRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendTaskRemindersToAllUsersAsync()
    {
        try
        {
            _logger.LogInformation("Starting task reminder process for all users");

            var users = await _userRepository.GetAllAsync();
            var activeUsers = users.Where(u => !u.IsDeleted).ToList();

            _logger.LogInformation("Processing {UserCount} active users for task reminders", activeUsers.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var user in activeUsers)
            {
                try
                {
                    var tasks = await _taskItemRepository.GetTasksByUserIdAsync(user.Id);
                    var today = DateTime.Now;
                    var incompleteTasks = tasks.Where(t => !t.IsCompleted
                                                        && !t.IsDeleted
                                                        && t.DueDate >= today).ToList();

                    if (incompleteTasks.Any())
                    {
                        var taskTitles = incompleteTasks.Select(t => t.Title).ToList();

                        _logger.LogDebug("User {UserId} ({Email}) has {TaskCount} incomplete tasks",
                            user.Id, user.Email, taskTitles.Count);

                        // EmailSenderService ile email gönder
                        await _emailService.SendTaskReminderAsync(user.Email, taskTitles);

                        successCount++;
                        _logger.LogDebug("Successfully sent reminder email to user {UserId} ({Email})",
                            user.Id, user.Email);
                    }
                    else
                    {
                        _logger.LogDebug("User {UserId} ({Email}) has no incomplete tasks, skipping",
                            user.Id, user.Email);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Failed to process task reminders for user {UserId} ({Email})",
                        user.Id, user.Email);
                    // Continue with next user - don't let one failure stop the entire process
                }
            }

            _logger.LogInformation("Task reminder process completed. Success: {SuccessCount}, Failures: {FailureCount}",
                successCount, failureCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send task reminders to all users");
            throw;
        }
    }
}