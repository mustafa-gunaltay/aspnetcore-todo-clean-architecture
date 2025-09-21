using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserViewModel>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetUserByIdQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<UserViewModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting user retrieval for ID {UserId}", request.UserId);
        
        try
        {
            // Get user by id
            _logger.LogDebug("Fetching user with ID {UserId} from repository", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("User with ID {UserId} not found after {Duration}ms", 
                    request.UserId, stopwatch.ElapsedMilliseconds);
                return Result<UserViewModel>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), calculating task count", 
                request.UserId, userEmail);

            var usersBySpecifiedId = await _uow.TaskItemRepository.GetTasksByUserIdAsync(user.Id, cancellationToken);
            var taskCount = usersBySpecifiedId.Count;
            
            _logger.LogDebug("User ID {UserId} has {TaskCount} associated tasks", request.UserId, taskCount);

            // Map to ViewModel
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                //TaskCount = user.TaskItems?.Count(t => !t.IsDeleted) ?? 0
                TaskCount = taskCount
            };

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved user ID {UserId} ({UserEmail}) with {TaskCount} tasks in {Duration}ms", 
                request.UserId, userEmail, taskCount, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for single user queries with task counting
            if (stopwatch.ElapsedMilliseconds > 300)
            {
                _logger.LogWarning("Slow query detected: GetUserById for ID {UserId} with task counting took {Duration}ms (threshold: 300ms)", 
                    request.UserId, stopwatch.ElapsedMilliseconds);
            }

            return Result<UserViewModel>.Success(userViewModel, "User retrieved successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during user retrieval for ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.UserId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<UserViewModel>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve user with ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<UserViewModel>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
}