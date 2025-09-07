using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetAllUsersQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<IReadOnlyList<UserViewModel>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all users
            var users = await _uow.UserRepository.GetAllAsync(cancellationToken);


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

            return Result<IReadOnlyList<UserViewModel>>.Success(userViewModels, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<UserViewModel>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}