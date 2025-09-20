using MediatR;
using Microsoft.Extensions.Logging;
using TodoBackend.Application.Features.BuildingBlocks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using System.Diagnostics;

namespace TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
{
    private readonly ITodoBackendUnitOfWork _uow;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(ITodoBackendUnitOfWork uow, ILogger<CreateCategoryCommandHandler> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation("Starting category creation process for name {CategoryName}", request.Name);
        
        try
        {
            // Check if category name is unique
            _logger.LogDebug("Checking if category name {CategoryName} is unique", request.Name);
            var isNameUnique = await _uow.CategoryRepository.IsNameUniqueAsync(request.Name, null, cancellationToken);
            if (!isNameUnique)
            {
                _logger.LogWarning("Category creation failed - duplicate name {CategoryName}", request.Name);
                return Result<int>.Failure($"Category with name '{request.Name}' already exists");
            }

            // Create new category
            _logger.LogDebug("Creating new category with name {CategoryName}", request.Name);
            var category = new Category(request.Name, request.Description);

            // Save to database
            await _uow.CategoryRepository.AddAsync(category, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            stopwatch.Stop();
            
            _logger.LogInformation("Category created successfully with ID {CategoryId} and name {CategoryName} in {Duration}ms", 
                category.Id, request.Name, stopwatch.ElapsedMilliseconds);

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
            _logger.LogWarning(ex, "Domain validation failed during category creation for name {CategoryName} after {Duration}ms: {ErrorMessage}", 
                request.Name, stopwatch.ElapsedMilliseconds, ex.Message);
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Category creation failed with exception for name {CategoryName} after {Duration}ms", 
                request.Name, stopwatch.ElapsedMilliseconds);
            return Result<int>.Failure($"Failed to create category: {ex.Message}");
        }
    }
}
