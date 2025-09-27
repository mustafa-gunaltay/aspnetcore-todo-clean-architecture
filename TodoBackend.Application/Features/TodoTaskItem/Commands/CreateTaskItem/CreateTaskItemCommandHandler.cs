using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Models;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Commands.CreateTaskItem;

public class CreateTaskItemCommandHandler : IRequestHandler<CreateTaskItemCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<CreateTaskItemCommandHandler> _logger;

    public CreateTaskItemCommandHandler(ITodoBackendUnitOfWork uow, ILogger<CreateTaskItemCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateTaskItemCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task creation process with title {Title} for user ID {UserId}", 
            request.Title, request.UserId);
        
        try
        {
            // Check if user exists and is active
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Task creation failed - user with ID {UserId} not found", request.UserId);
                return Result<int>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), creating task with title {Title}", 
                request.UserId, userEmail, request.Title);

            // Create TaskItem using domain constructor
            var taskItem = new TaskItem(
                title: request.Title,
                description: request.Description,
                priority: request.Priority,
                dueDate: request.DueDate,
                userId: request.UserId
            );

            _logger.LogDebug("Created task item with priority {Priority} and due date {DueDate} for user ID {UserId}", 
                request.Priority, request.DueDate, request.UserId);

            // Save TaskItem
            await _uow.TaskItemRepository.AddAsync(taskItem, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Task created successfully with ID {TaskId} and title {Title} for user ID {UserId} ({UserEmail}) in {Duration}ms", 
                taskItem.Id, request.Title, request.UserId, userEmail, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow task creation detected: {Duration}ms for task {Title} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.Title);
            }

            return Result<int>.Success(taskItem.Id, "Task created successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task creation for title {Title} and user ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.Title, request.UserId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<int>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Task creation failed with exception for title {Title} and user ID {UserId} after {Duration}ms", 
                request.Title, request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<int>.Failure($"Failed to create task: {ex.Message}");
        }
    }
}