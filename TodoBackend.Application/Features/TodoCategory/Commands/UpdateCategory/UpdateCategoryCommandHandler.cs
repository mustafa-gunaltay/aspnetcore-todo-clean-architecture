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

    public UpdateCategoryCommandHandler(ITodoBackendUnitOfWork uow, ILogger<UpdateCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category update process for ID {CategoryId} with new name {CategoryName}", 
            request.CategoryId, request.Name);
        
        try
        {
            // Check if category exists
            _logger.LogDebug("Checking if category with ID {CategoryId} exists", request.CategoryId);
            var category = await _uow.CategoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
            if (category is null)
            {
                _logger.LogWarning("Category update failed - category with ID {CategoryId} not found", request.CategoryId);
                return Result.Failure("Category not found");
            }

            var oldName = category.Name;
            var oldDescription = category.Description;

            // Check if new name is unique (exclude current category)
            _logger.LogDebug("Checking if new name {CategoryName} is unique for category ID {CategoryId}", 
                request.Name, request.CategoryId);
            var isNameUnique = await _uow.CategoryRepository.IsNameUniqueAsync(request.Name, request.CategoryId, cancellationToken);
            if (!isNameUnique)
            {
                _logger.LogWarning("Category update failed - duplicate name {CategoryName} for category ID {CategoryId}", 
                    request.Name, request.CategoryId);
                return Result.Failure("Category name must be unique");
            }

            // Track changes
            var changes = new List<string>();
            if (oldName != request.Name)
                changes.Add($"Name: '{oldName}' -> '{request.Name}'");
            if (oldDescription != request.Description)
                changes.Add($"Description: '{oldDescription}' -> '{request.Description}'");

            // Update properties using domain methods
            _logger.LogDebug("Updating category ID {CategoryId} with changes: {Changes}", 
                request.CategoryId, string.Join(", ", changes));
            category.Rename(request.Name);
            category.SetDescription(request.Description);

            // Save changes
            await _uow.CategoryRepository.UpdateAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Category ID {CategoryId} updated successfully in {Duration}ms with changes: {Changes}", 
                request.CategoryId, stopwatch.ElapsedMilliseconds, string.Join(", ", changes));

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
            _logger.LogWarning(dex, "Domain validation failed during category update for ID {CategoryId} after {Duration}ms: {ErrorMessage}", 
                request.CategoryId, stopwatch.ElapsedMilliseconds, dex.Message);
            return Result.Failure(dex.Message);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Category update failed with exception for ID {CategoryId} after {Duration}ms", 
                request.CategoryId, stopwatch.ElapsedMilliseconds);
            return Result.Failure($"Failed to update category: {ex.Message}");
        }
    }
}
