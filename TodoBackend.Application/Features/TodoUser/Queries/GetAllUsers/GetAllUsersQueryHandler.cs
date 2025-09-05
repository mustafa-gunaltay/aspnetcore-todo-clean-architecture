using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.Interfaces;

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

            // Map to ViewModels
            var userViewModels = users.Select(user => new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                TaskCount = user.TaskItems?.Count(t => !t.IsDeleted) ?? 0
            }).ToList();

            return Result<IReadOnlyList<UserViewModel>>.Success(userViewModels, "Users retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<UserViewModel>>.Failure($"Failed to retrieve users: {ex.Message}");
        }
    }
}