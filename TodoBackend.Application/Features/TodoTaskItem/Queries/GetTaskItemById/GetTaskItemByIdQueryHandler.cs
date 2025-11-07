using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.DomainExceptions;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemById;

public class GetTaskItemByIdQueryHandler : IRequestHandler<GetTaskItemByIdQuery, Result<TaskItemViewModel>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetTaskItemByIdQueryHandler> _logger;

    public GetTaskItemByIdQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetTaskItemByIdQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<TaskItemViewModel>> Handle(GetTaskItemByIdQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting task retrieval for ID {TaskItemId}", request.TaskItemId);
        
        try
        {
            _logger.LogDebug("Fetching task with ID {TaskItemId} including user and categories from repository", request.TaskItemId);
            var taskItem = await _uow.TaskItemRepository.GetByIdWithDetailsAsync(request.TaskItemId, cancellationToken);
            if (taskItem is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("Task with ID {TaskItemId} not found after {Duration}ms", 
                    request.TaskItemId, stopwatch.ElapsedMilliseconds);
                return Result<TaskItemViewModel>.Failure("Task not found");
            }

            var taskTitle = taskItem.Title; // Store for logging
            var userEmail = taskItem.User?.Email ?? "Unknown"; // Store for logging
            
            _logger.LogDebug("Found task ID {TaskItemId} ({TaskTitle}) belonging to user {UserEmail} with {CategoryCount} associated categories", 
                taskItem.Id, taskTitle, userEmail, taskItem.TaskItemCategories.Count);

            // Map to ViewModel
            var viewModel = new TaskItemViewModel
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Priority = taskItem.Priority,
                DueDate = taskItem.DueDate,
                CompletedAt = taskItem.CompletedAt,
                IsCompleted = taskItem.IsCompleted,
                UserId = taskItem.UserId,
                UserEmail = taskItem.User?.Email,
                CreatedAt = taskItem.CreatedAt,
                UpdatedAt = taskItem.UpdatedAt,
                Categories = taskItem.TaskItemCategories
                    ?.Where(tc => !tc.IsDeleted)
                    .Select(tc => new CategorySummaryViewModel
                    {
                        Id = tc.CategoryId,
                        Name = tc.Category?.Name ?? "Unknown"
                    }).ToList() ?? new List<CategorySummaryViewModel>()
            };

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved task ID {TaskItemId} ({TaskTitle}) for user {UserEmail} - Status: {Status} in {Duration}ms", 
                request.TaskItemId, taskTitle, userEmail, taskItem.IsCompleted ? "Completed" : "Pending", stopwatch.ElapsedMilliseconds);

            // Performance monitoring for single entity queries
            if (stopwatch.ElapsedMilliseconds > 200)
            {
                _logger.LogWarning("Slow query detected: GetTaskItemById for ID {TaskItemId} took {Duration}ms (threshold: 200ms)", 
                    request.TaskItemId, stopwatch.ElapsedMilliseconds);
            }

            return Result<TaskItemViewModel>.Success(viewModel, "Task retrieved successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during task retrieval for ID {TaskItemId} after {Duration}ms: {ErrorMessage}", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<TaskItemViewModel>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve task with ID {TaskItemId} after {Duration}ms", 
                request.TaskItemId, stopwatch.ElapsedMilliseconds);
            return Result<TaskItemViewModel>.Failure($"Failed to retrieve task: {ex.Message}");
        }
    }
}
