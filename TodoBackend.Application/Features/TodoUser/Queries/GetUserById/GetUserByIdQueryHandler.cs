using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoUser.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserViewModel>>
{
    private readonly ITodoBackendUnitOfWork _uow;

    public GetUserByIdQueryHandler(ITodoBackendUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Result<UserViewModel>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user by id
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result<UserViewModel>.Failure("User not found");
            }

            // Map to ViewModel
            var userViewModel = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                TaskCount = user.TaskItems?.Count(t => !t.IsDeleted) ?? 0
            };

            return Result<UserViewModel>.Success(userViewModel, "User retrieved successfully");
        }
        catch (DomainException dex)
        {
            return Result<UserViewModel>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result<UserViewModel>.Failure($"Failed to retrieve user: {ex.Message}");
        }
    }
}