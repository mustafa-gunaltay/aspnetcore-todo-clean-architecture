using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Models;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetAllUsersQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<UserViewModel>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting retrieval of all users");
        
        try
        {
            // Get all users
            _logger.LogDebug("Fetching all users from repository");
            var users = await _uow.UserRepository.GetAllAsync(cancellationToken);

            _logger.LogDebug("Retrieved {UserCount} users from database, calculating task counts", users.Count);

            // Map to ViewModels with repository-based TaskCount calculation
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                // Repository pattern kullanarak TaskCount hesapla
                var userTasks = await _uow.TaskItemRepository.GetTasksByUserIdAsync(user.Id, cancellationToken);
                var taskCount = userTasks.Count;

                var userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    TaskCount = taskCount
                };

                userViewModels.Add(userViewModel);
            }

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {UserCount} users with task counts in {Duration}ms", 
                userViewModels.Count, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for user collection queries with task counting
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: GetAllUsers with task counting took {Duration}ms for {UserCount} users (threshold: 1000ms). Consider optimizing task count calculation.", 
                    stopwatch.ElapsedMilliseconds, userViewModels.Count);
            }

            return Result<IReadOnlyList<UserViewModel>>.Success(userViewModels, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve users after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<UserViewModel>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}