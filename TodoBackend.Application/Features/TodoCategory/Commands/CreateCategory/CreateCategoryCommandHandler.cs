using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Models;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;
    private readonly IAuthenticationValidationService _authValidationService;

    public CreateCategoryCommandHandler(
        ITodoBackendUnitOfWork uow, 
        ILogger<CreateCategoryCommandHandler> logger, 
        IAuthenticationValidationService authValidationService)
    {
        _uow = uow;
        _logger = logger;
        _authValidationService = authValidationService;
    }

    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category creation process for name {CategoryName} for user ID {UserId}", 
            request.Name, request.UserId);
        
        try
        {
            // AUTHENTICATION VALIDATION - Clean Architecture compliant
            var authResult = _authValidationService.ValidateUserAuthentication(request.UserId, "category creation");
            if (!authResult.IsSuccessful)
            {
                stopwatch.Stop();
                return Result<int>.Failure(authResult.ErrorMessage);
            }

            // Check if user exists and is active
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Category creation failed - user with ID {UserId} not found", request.UserId);
                return Result<int>.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), creating category with name {CategoryName}", 
                request.UserId, userEmail, request.Name);

            // Check if category name is unique for this user
            _logger.LogDebug("Checking if category name {CategoryName} is unique for user ID {UserId}", request.Name, request.UserId);
            var isNameUnique = await _uow.CategoryRepository.IsNameUniqueForUserAsync(request.Name, request.UserId, null, cancellationToken);
            if (!isNameUnique)
            {
                _logger.LogWarning("Category creation failed - duplicate name {CategoryName} for user ID {UserId}", request.Name, request.UserId);
                return Result<int>.Failure($"You already have a category with name '{request.Name}'");
            }

            // Create new category
            _logger.LogDebug("Creating new category with name {CategoryName} for user ID {UserId}", request.Name, request.UserId);
            var category = new Category(request.Name, request.Description, request.UserId);

            // Save to database
            await _uow.CategoryRepository.AddAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Category created successfully with ID {CategoryId} and name {CategoryName} for user ID {UserId} ({UserEmail}) in {Duration}ms", 
                category.Id, request.Name, request.UserId, userEmail, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow category creation detected: {Duration}ms for category {CategoryName} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.Name);
            }

            // Return success with the new category's ID
            return Result<int>.Success(category.Id, "Category created successfully");
        }
        catch (DomainException ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Domain validation failed during category creation for name {CategoryName} and user ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.Name, request.UserId, stopwatch.ElapsedMilliseconds, ex.Message);
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Category creation failed with exception for name {CategoryName} and user ID {UserId} after {Duration}ms", 
                request.Name, request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
    }
}
