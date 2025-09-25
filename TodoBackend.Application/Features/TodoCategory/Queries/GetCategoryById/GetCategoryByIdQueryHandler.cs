using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Application.ViewModels;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, Result<CategoryViewModel>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

    public GetCategoryByIdQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetCategoryByIdQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<CategoryViewModel>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category retrieval for ID {CategoryId}", request.CategoryId);
        
        try
        {
            _logger.LogDebug("Fetching category with ID {CategoryId} including user information from repository", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdWithUserAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                stopwatch.Stop();
                _logger.LogWarning("Category with ID {CategoryId} not found after {Duration}ms", 
                    request.CategoryId, stopwatch.ElapsedMilliseconds);
                return Result<CategoryViewModel>.Failure("Category not found");
            }

            var categoryName = category.Name; // Store for logging
            var userEmail = category.User?.Email ?? "Unknown"; // Store for logging
            
            _logger.LogDebug("Found category ID {CategoryId} ({CategoryName}) belonging to user {UserEmail} with {TaskCount} associated tasks", 
                category.Id, categoryName, userEmail, category.TaskItemCategories.Count);

            var viewModel = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                TaskCount = category.TaskItemCategories.Count,
                User = category.User != null ? new UserSummaryViewModel
                {
                    Id = category.User.Id,
                    Email = category.User.Email
                } : null,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved category ID {CategoryId} ({CategoryName}) for user {UserEmail} in {Duration}ms", 
                request.CategoryId, categoryName, userEmail, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for single entity queries
            if (stopwatch.ElapsedMilliseconds > 200)
            {
                _logger.LogWarning("Slow query detected: GetCategoryById for ID {CategoryId} took {Duration}ms (threshold: 200ms)", 
                    request.CategoryId, stopwatch.ElapsedMilliseconds);
            }

            return Result<CategoryViewModel>.Success(viewModel, "Category retrieved successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during category retrieval for ID {CategoryId} after {Duration}ms: {ErrorMessage}", 
                request.CategoryId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<CategoryViewModel>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve category with ID {CategoryId} after {Duration}ms", 
                request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result<CategoryViewModel>.Failure($"Failed to retrieve category: {ex.Message}");
        }
    }
}
