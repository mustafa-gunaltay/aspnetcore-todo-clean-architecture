using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Models;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoUser.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(ITodoBackendUnitOfWork uow, IPasswordHasher passwordHasher, ILogger<CreateUserCommandHandler> logger)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting user creation process for email {Email}", request.Email);
        
        try
        {
            // Check if email is unique
            _logger.LogDebug("Checking if email {Email} is unique", request.Email);
            var isEmailUnique = await _uow.UserRepository.IsEmailUniqueAsync(request.Email, null, cancellationToken);
            if (!isEmailUnique)
            {
                _logger.LogWarning("User creation failed - duplicate email {Email}", request.Email);
                return Result<int>.Failure("Email address is already in use");
            }

            // Hash the password
            _logger.LogDebug("Hashing password for user {Email}", request.Email);
            var (hash, salt) = _passwordHasher.Hash(request.Password);

            // Create user using domain factory method with hash and salt
            _logger.LogDebug("Creating new user with email {Email}", request.Email);
            var user = User.Create(request.Email, hash, salt);

            // Save user
            await _uow.UserRepository.AddAsync(user, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("User created successfully with ID {UserId} and email {Email} in {Duration}ms", 
                user.Id, request.Email, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow user creation detected: {Duration}ms for email {Email} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.Email);
            }

            return Result<int>.Success(user.Id, "User created successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during user creation for email {Email} after {Duration}ms: {ErrorMessage}", 
                request.Email, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<int>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "User creation failed with exception for email {Email} after {Duration}ms", 
                request.Email, stopwatch.ElapsedMilliseconds);
            return Result<int>.Failure($"Failed to create user: {ex.Message}");
        }
    }
}