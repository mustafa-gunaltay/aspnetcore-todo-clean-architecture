using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoCategory.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(ITodoBackendUnitOfWork uow, ILogger<DeleteCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category deletion process for ID {CategoryId}", request.CategoryId);
        
        try
        {
            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category deletion failed - category with ID {CategoryId} not found", request.CategoryId);
                return Result.Failure("Category not found");
            }

            var categoryName = category.Name; // Store for logging

            // Check if category has active tasks
            _logger.LogDebug("Checking if category ID {CategoryId} ({CategoryName}) has active tasks", 
                request.CategoryId, categoryName);
            var hasActiveTasks = await _uow.CategoryRepository.HasActiveTasksAsync(request.CategoryId, cancellationToken);
            if (hasActiveTasks)
            {
                _logger.LogWarning("Category deletion failed - category ID {CategoryId} ({CategoryName}) has active tasks", 
                    request.CategoryId, categoryName);
                return Result.Failure("Cannot delete category with active tasks");
            }

            // Delete the category (soft delete)
            _logger.LogDebug("Performing soft delete for category ID {CategoryId} ({CategoryName})", 
                request.CategoryId, categoryName);
            await _uow.CategoryRepository.DeleteAsync(category, cancellationToken);

            // Save changes
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Category ID {CategoryId} ({CategoryName}) deleted successfully in {Duration}ms", 
                request.CategoryId, categoryName, stopwatch.ElapsedMilliseconds);

            // Performance warning
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow category deletion detected: {Duration}ms for category ID {CategoryId} (threshold: 1000ms)", 
                    stopwatch.ElapsedMilliseconds, request.CategoryId);
            }

            return Result.Success("Category deleted successfully");
        }
        catch (DomainException dex)
        {
            stopwatch.Stop();
            _logger.LogWarning(dex, "Domain validation failed during category deletion for ID {CategoryId} after {Duration}ms: {ErrorMessage}", 
                request.CategoryId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Category deletion failed with exception for ID {CategoryId} after {Duration}ms", 
                request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to delete category: {ex.Message}");
        }
    }
}