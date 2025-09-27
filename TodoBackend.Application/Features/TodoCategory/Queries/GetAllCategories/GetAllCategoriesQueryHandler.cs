using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Application.ViewModels;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Out;

namespace TodoBackend.Application.Features.TodoCategory.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryViewModel>>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<GetAllCategoriesQueryHandler> _logger;

    public GetAllCategoriesQueryHandler(ITodoBackendUnitOfWork uow, ILogger<GetAllCategoriesQueryHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<CategoryViewModel>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting retrieval of all categories with user information");
        
        try
        {
            // Get categories with task count and user information using the repository method
            _logger.LogDebug("Fetching categories with task count and user information from repository");
            var categories = await _uow.CategoryRepository.GetCategoriesWithTaskCountAsync(cancellationToken);

            _logger.LogDebug("Retrieved {CategoryCount} categories from database", categories.Count);

            // Map to ViewModels
            var categoryViewModels = categories
                .Select(c => new CategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    TaskCount = c.TaskItemCategories.Count,
                    User = c.User != null ? new UserSummaryViewModel
                    {
                        Id = c.User.Id,
                        Email = c.User.Email
                    } : null,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt
                })
                .ToList();

            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {CategoryCount} categories with user information in {Duration}ms", 
                categoryViewModels.Count, stopwatch.ElapsedMilliseconds);

            // Performance monitoring for queries
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning("Slow query detected: GetAllCategories took {Duration}ms (threshold: 500ms). Consider optimizing the query.", 
                    stopwatch.ElapsedMilliseconds);
            }

            return Result<IReadOnlyList<CategoryViewModel>>.Success(categoryViewModels, "Categories retrieved successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during category retrieval after {Duration}ms: {ErrorMessage}", 
                stopwatch.ElapsedMilliseconds, dex.Message);
            return Result<IReadOnlyList<CategoryViewModel>>.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve categories after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
            return Result<IReadOnlyList<CategoryViewModel>>.Failure($"Failed to retrieve categories: {ex.Message}");
        }
    }
}