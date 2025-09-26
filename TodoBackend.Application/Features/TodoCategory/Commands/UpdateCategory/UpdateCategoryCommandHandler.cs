using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoCategory.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;
    private readonly IAuthenticationValidationService _authValidationService;

    public UpdateCategoryCommandHandler(
        ITodoBackendUnitOfWork uow, 
        ILogger<UpdateCategoryCommandHandler> logger, 
        IAuthenticationValidationService authValidationService)
    {
        _uow = uow;
        _logger = logger;
        _authValidationService = authValidationService;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category update process for ID {CategoryId} by user ID {UserId}", 
            request.CategoryId, request.UserId);
        
        try
        {
            // AUTHENTICATION VALIDATION - Clean Architecture compliant
            var authResult = _authValidationService.ValidateUserAuthentication(request.UserId, "category update");
            if (!authResult.IsSuccessful)
            {
                stopwatch.Stop();
                return Result.Failure(authResult.ErrorMessage);
            }

            // Check if user exists and is active
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("Category update failed - user with ID {UserId} not found", request.UserId);
                return Result.Failure("User not found");
            }

            var userEmail = user.Email; // Store for logging

            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category update failed - category with ID {CategoryId} not found", request.CategoryId);
                return Result.Failure("Category not found");
            }

            // Check if the category belongs to the requesting user (security check)
            if (category.UserId != request.UserId)
            {
                _logger.LogWarning("Category update failed - user ID {UserId} attempted to update category ID {CategoryId} which belongs to user ID {CategoryUserId}", 
                    request.UserId, request.CategoryId, category.UserId);
                return Result.Failure("You can only update your own categories");
            }

            _logger.LogDebug("Found category ID {CategoryId} belonging to user ID {UserId} ({UserEmail})", 
                request.CategoryId, request.UserId, userEmail);

            var oldName = category.Name;
            var oldDescription = category.Description;
            var changes = new List<string>();

            // SELECTIVE UPDATE PATTERN

            // Name güncelleme - sadece null de?ilse
            if (request.Name != null)
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    _logger.LogWarning("Category update failed - empty name provided for category ID {CategoryId}", request.CategoryId);
                    return Result.Failure("Category name cannot be empty");
                }

                // Check if new name is unique for this user (exclude current category)
                _logger.LogDebug("Checking if new name {CategoryName} is unique for user ID {UserId}, excluding category ID {CategoryId}", 
                    request.Name, request.UserId, request.CategoryId);
                var isNameUnique = await _uow.CategoryRepository.IsNameUniqueForUserAsync(request.Name, request.UserId, request.CategoryId, cancellationToken);
                if (!isNameUnique)
                {
                    _logger.LogWarning("Category update failed - duplicate name {CategoryName} for user ID {UserId}", 
                        request.Name, request.UserId);
                    return Result.Failure($"You already have a category with name '{request.Name}'");
                }

                category.Rename(request.Name);
                changes.Add($"Name: '{oldName}' -> '{request.Name}'");
            }

            // Description güncelleme - sadece null de?ilse
            if (request.Description != null)
            {
                if (string.IsNullOrWhiteSpace(request.Description))
                {
                    _logger.LogWarning("Category update failed - empty description provided for category ID {CategoryId}", request.CategoryId);
                    return Result.Failure("Category description cannot be empty");
                }

                category.SetDescription(request.Description);
                changes.Add($"Description: '{oldDescription}' -> '{request.Description}'");
            }

            // Hiç de?i?iklik yoksa
            if (!changes.Any())
            {
                _logger.LogInformation("No changes detected for category ID {CategoryId}, skipping update", request.CategoryId);
                return Result.Success("No changes were made to the category");
            }

            _logger.LogDebug("Updating category ID {CategoryId} with changes: {Changes}", 
                request.CategoryId, string.Join(", ", changes));

            // Save changes
            await _uow.CategoryRepository.UpdateAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Category ID {CategoryId} updated successfully by user ID {UserId} ({UserEmail}) in {Duration}ms with changes: {Changes}", 
                request.CategoryId, request.UserId, userEmail, stopwatch.ElapsedMilliseconds, string.Join(", ", changes));

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow category update detected: {Duration}ms for category ID {CategoryId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.CategoryId);
            }

            return Result.Success("Category updated successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during category update for ID {CategoryId} by user ID {UserId} after {Duration}ms: {ErrorMessage}", 
                request.CategoryId, request.UserId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Category update failed with exception for ID {CategoryId} by user ID {UserId} after {Duration}ms", 
                request.CategoryId, request.UserId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to update category: {ex.Message}");
        }
    }
}
