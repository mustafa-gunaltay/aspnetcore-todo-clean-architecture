using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.Specifications.TaskItem;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Application.Features.TodoTaskItem.Queries.GetTaskItemsByFilter;

public class GetTaskItemsByFilterQueryHandler : IRequestHandler<GetTaskItemsByFilterQuery, Result<PagedList<TaskItemViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetTaskItemsByFilterQueryHandler> _logger;
    private readonly ILogger<GetTaskItemsByFilterQuerySpecification> _specificationLogger;

    public GetTaskItemsByFilterQueryHandler(
        ITodoBackendUnitOfWork uow, 
        ILogger<GetTaskItemsByFilterQueryHandler> logger,

        // GetTaskItemsByFilterQuerySpecification için logger
        ILogger<GetTaskItemsByFilterQuerySpecification> specificationLogger)
    {
        _uow = uow;
        _logger = logger;
        _specificationLogger = specificationLogger;
    }

    public async Task<Result<PagedList<TaskItemViewModel>>> Handle(GetTaskItemsByFilterQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting filtered task retrieval with pagination for user ID {UserId} - PageSize: {PageSize}, PageNumber: {PageNumber}", 
            request.UserId, request.PageSize, request.PageNumber);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Filtered task retrieval failed - user with ID {UserId} not found", request.UserId);
                return Result<PagedList<TaskItemViewModel>>.Failure("User not found");
            }

            var userEmail = user.Email;
            
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), creating specification", 
                request.UserId, userEmail);

            // Create specification with its properly typed logger injected from DI
            var specification = new GetTaskItemsByFilterQuerySpecification(request, _specificationLogger);

            // Execute query with specification
            var (totalCount, data) = await _uow.TaskItemRepository.ListAsync(specification, cancellationToken);

            _logger.LogDebug("Retrieved {DataCount} tasks from repository (TotalCount: {TotalCount}) for user ID {UserId}", 
                data.Count, totalCount, request.UserId);

            // Map to ViewModels
            var taskItemViewModels = data.Select(task => new TaskItemViewModel
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                IsCompleted = task.IsCompleted,
                UserId = task.UserId,
                UserEmail = task.User?.Email,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Categories = task.TaskItemCategories
                    ?.Where(tc => !tc.IsDeleted)
                    .Select(tc => new CategorySummaryViewModel
                    {
                        Id = tc.CategoryId,
                        Name = tc.Category?.Name ?? "Unknown"
                    }).ToList() ?? new List<CategorySummaryViewModel>()
            }).ToList();

            // Create PagedList
            var pagedList = PagedList<TaskItemViewModel>.Create(
                pageSize: request.PageSize,
                pageNumber: request.PageNumber,
                totalCount: totalCount,
                data: taskItemViewModels
            );

            stopwatch.Stop();
            
            var totalPages = Math.Ceiling((double)totalCount / request.PageSize);
            _logger.LogInformation("Successfully retrieved {TaskCount} of {TotalCount} filtered tasks for user ID {UserId} ({UserEmail}) - Page {PageNumber}/{TotalPages} in {Duration}ms", 
                taskItemViewModels.Count, 
                totalCount, 
                request.UserId, 
                userEmail, 
                request.PageNumber, 
                totalPages,
                stopwatch.ElapsedMilliseconds);

            // Performance monitoring
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow filtered query detected: {Duration}ms for user ID {UserId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.UserId);
            }

            return Result<PagedList<TaskItemViewModel>>.Success(
                pagedList,
                $"Retrieved page {request.PageNumber} with {taskItemViewModels.Count} of {totalCount} filtered task items successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve filtered task items for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<PagedList<TaskItemViewModel>>.Failure($"Failed to retrieve filtered task items: {ex.Message}");
        }
    }
}
