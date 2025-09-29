using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Actors.Messages;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Infrastructure.Actors;

/// <summary>
/// Kullan?c?n?n task'lar?n? getirme i?lemlerinden sorumlu Actor
/// Mevcut Repository pattern'ini kullanarak task verilerini getirir
/// </summary>
public class TaskFetcherActor : ReceiveActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TaskFetcherActor> _logger;

    public TaskFetcherActor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        using var scope = serviceProvider.CreateScope();
        _logger = scope.ServiceProvider.GetRequiredService<ILogger<TaskFetcherActor>>();

        // Message handling patterns
        ReceiveAsync<FetchUserTasks>(HandleFetchUserTasks);

        _logger.LogDebug("TaskFetcherActor created: {ActorPath}", Self.Path);
    }

    /// <summary>
    /// Kullan?c?n?n task'lar?n? getirme mesaj?n? i?ler
    /// Mevcut EmailNotificationService'deki ayn? business logic'i kullan?r
    /// </summary>
    private async Task HandleFetchUserTasks(FetchUserTasks message)
    {
        try
        {
            _logger.LogDebug("Fetching tasks for user {UserId}", message.UserId);

            using var scope = _serviceProvider.CreateScope();
            var taskRepository = scope.ServiceProvider.GetRequiredService<ITaskItemRepository>();

            // Mevcut EmailNotificationService'deki ayn? logic
            var tasks = await taskRepository.GetTasksByUserIdAsync(message.UserId);
            var today = DateTime.Now;
            var incompleteTasks = tasks.Where(t => !t.IsCompleted
                                                && !t.IsDeleted
                                                && t.DueDate >= today).ToList();

            if (incompleteTasks.Any())
            {
                var taskTitles = incompleteTasks.Select(t => t.Title).ToList();

                _logger.LogDebug("User {UserId} has {TaskCount} incomplete tasks", 
                    message.UserId, taskTitles.Count);

                // Task'lar bulundu bildirimi gönder
                Sender.Tell(new TasksRetrieved(message.UserId, taskTitles));
            }
            else
            {
                _logger.LogDebug("User {UserId} has no incomplete tasks", message.UserId);
                
                // Bo? liste gönder (task yok demek)
                Sender.Tell(new TasksRetrieved(message.UserId, new List<string>()));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch tasks for user {UserId}", message.UserId);
            
            // Hata durumunda bo? liste gönder ve parent'a hata bildir
            Sender.Tell(new TasksRetrieved(message.UserId, new List<string>()));
            Context.Parent.Tell(new UserReminderCompleted(message.UserId, false, ex.Message));
        }
    }

    /// <summary>
    /// Actor Props factory method
    /// </summary>
    public static Props Props(IServiceProvider serviceProvider) =>
        Akka.Actor.Props.Create(() => new TaskFetcherActor(serviceProvider));

    protected override void PreStart()
    {
        _logger.LogDebug("TaskFetcherActor starting: {ActorPath}", Self.Path);
        base.PreStart();
    }

    protected override void PostStop()
    {
        _logger.LogDebug("TaskFetcherActor stopped: {ActorPath}", Self.Path);
        base.PostStop();
    }
}