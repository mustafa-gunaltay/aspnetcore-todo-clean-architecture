using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Interfaces.Outside;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoriesByUserId;

public class GetCategoriesByUserIdQueryHandler : IRequestHandler<GetCategoriesByUserIdQuery, Result<IReadOnlyList<CategoryViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetCategoriesByUserIdQueryHandler> _logger;
    private readonly IAuthenticationValidationService _authValidationService;

    public GetCategoriesByUserIdQueryHandler(
        ITodoBackendUnitOfWork uow, 
        ILogger<GetCategoriesByUserIdQueryHandler> logger, 
        IAuthenticationValidationService authValidationService)
    {
        _uow = uow;
        _logger = logger;
        _authValidationService = authValidationService;    
    }

    public async Task<Result<IReadOnlyList<CategoryViewModel>>> Handle(GetCategoriesByUserIdQuery request, CancellationToken cancellationToken) 
    { 
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category retrieval for user ID {UserId}", request.UserId);
        
        try
        {
            // Check if user exists
            _logger.LogDebug("Checking if user with ID {UserId} exists", request.UserId);
            var user = await _uow.UserRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("User with ID {UserId} not found after {Duration}ms",
                    request.UserId, stopwatch.ElapsedMilliseconds);
                return Result<IReadOnlyList<CategoryViewModel>>.Failure("User not found");
            }

            // AUTHENTICATION VALIDATION - Clean Architecture compliant
            var authResult = _authValidationService.ValidateUserAuthentication(request.UserId, "get categories by user");
            if (!authResult.IsSuccessful)
            {
                stopwatch.Stop();
                _logger.LogWarning("Authentication failed for user ID {UserId}: {ErrorMessage}", 
                    request.UserId, authResult.ErrorMessage);
                return Result<IReadOnlyList<CategoryViewModel>>.Failure(authResult.ErrorMessage);
            }

            var userEmail = user.Email; // Store for logging
            _logger.LogDebug("Found user ID {UserId} ({UserEmail}), fetching categories", 
                request.UserId, userEmail);

            // Get categories for this specific user
            var categories = await _uow.CategoryRepository.GetCategoriesForUserAsync(request.UserId, cancellationToken);

            _logger.LogDebug("Retrieved {CategoryCount} categories for user ID {UserId}", 
                categories.Count, request.UserId);

            // Map to CategoryViewModels
            var categoryViewModels = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                TaskCount = c.TaskItemCategories?.Count ?? 0,
                User = c.User != null ? new UserSummaryViewModel
                {
                    Id = c.User.Id,
                    Email = c.User.Email
                } : null,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList();

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {CategoryCount} categories for user ID {UserId} ({UserEmail}) in {Duration}ms", 
                categoryViewModels.Count, request.UserId, userEmail, stopwatch.ElapsedMilliseconds);

            // Performance monitoring
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow query detected: GetCategoriesByUserId for user ID {UserId} took {Duration}ms (threshold: 500ms)", 
                    request.UserId, stopwatch.ElapsedMilliseconds);
            }

            return Result<IReadOnlyList<CategoryViewModel>>.Success(
                categoryViewModels,
                $"Retrieved {categoryViewModels.Count} categories for user '{userEmail}' successfully");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve categories for user ID {UserId} after {Duration}ms", 
                request.UserId, stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<CategoryViewModel>>.Failure($"Failed to retrieve categories for user: {ex.Message}");
        }
    }
}
