using MediatR;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;

namespace TodoBackend.Application.Features.TodoUser.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if email is unique
            var isEmailUnique = await _uow.UserRepository.IsEmailUniqueAsync(request.Email, null, cancellationToken);
            if (!isEmailUnique)
            {
                return Result<int>.Failure("Email address is already in use");
            }

            // Hash the password
            var (hash, salt) = _passwordHasher.Hash(request.Password);

            // Create user using domain factory method with hash and salt
            var user = User.Create(request.Email, hash, salt);

            // Save user
            await _uow.UserRepository.AddAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<int>.Success(user.Id, "User created successfully");
        }
        catch (DomainException dex)
        {
            return Result<int>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}