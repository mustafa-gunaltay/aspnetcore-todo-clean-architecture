using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Application.Features.TodoUser.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user exists
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure("User not found");
            }

            // Check if new email is unique (exclude current user)
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var isEmailUnique = await _uow.UserRepository.IsEmailUniqueAsync(request.Email, request.UserId, cancellationToken);
                if (!isEmailUnique)
                {
                    return Result.Failure("Email address is already in use");
                }

                // Update email using domain method
                user.ChangeEmail(request.Email);
            }

            // Update password only if provided using domain methods
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                // Hash the new password
                var (hash, salt) = _passwordHasher.Hash(request.Password);

                // Apply new password using domain method
                user.ApplyNewPassword(hash, salt);
            }

            // Save changes
            await _uow.UserRepository.UpdateAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result.Success("User updated successfully");
        }
        catch (DomainException dex)
        {
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to update user: {ex.Message}");
        }
    }
}