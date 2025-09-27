using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models.BuildingBlocks;
using System.Diagnostics;
using TodoBackend.Domain.Interfaces.Inside;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity> where TEntity : Entity
{
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<Repository<TEntity>> _logger;
    
    public Repository(TodoBackendDbContext dbContext, ICurrentUser currentUser, ILogger<Repository<TEntity>> logger) : base(dbContext, logger)
    {
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id;
        
        _logger.LogDebug("Starting add operation for {EntityType} with ID {EntityId}", entityType, entityId);
        
        try
        {
            if (entity is AuditableEntity trackable)
            {
                trackable.Created(_currentUser.UserName);
                _logger.LogDebug("Applied audit tracking for {EntityType} ID {EntityId} by user {UserName}", 
                    entityType, entityId, _currentUser.UserName);
            }
            
            await Set.AddAsync(entity, cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully added {EntityType} with ID {EntityId} in {Duration}ms", 
                entityType, entityId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to add {EntityType} with ID {EntityId} after {Duration}ms", 
                entityType, entityId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id;
        
        _logger.LogDebug("Starting update operation for {EntityType} with ID {EntityId}", entityType, entityId);
        
        try
        {
            if (entity is AuditableEntity trackable)
            {
                trackable.Updated(_currentUser.UserName);
                _logger.LogDebug("Applied audit tracking for {EntityType} ID {EntityId} update by user {UserName}", 
                    entityType, entityId, _currentUser.UserName);
            }
            
            await Task.Run(() => Set.Update(entity), cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully updated {EntityType} with ID {EntityId} in {Duration}ms", 
                entityType, entityId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to update {EntityType} with ID {EntityId} after {Duration}ms", 
                entityType, entityId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        var entityId = entity.Id;
        var deleteType = entity is AuditableEntity ? "soft" : "hard";
        
        _logger.LogDebug("Starting {DeleteType} delete operation for {EntityType} with ID {EntityId}", 
            deleteType, entityType, entityId);
        
        try
        {
            if (entity is AuditableEntity trackable)
            {
                // AuditableEntity ise soft delete (mantıksal silme) yapar (isDeleted=true)
                trackable.Deleted(_currentUser.UserName);
                await Task.Run(() => Set.Update(entity), cancellationToken);
                
                _logger.LogDebug("Applied soft delete for {EntityType} ID {EntityId} by user {UserName}", 
                    entityType, entityId, _currentUser.UserName);
            }
            else
            {
                // AuditableEntity değilse hard delete (fiziksel silme) yapar
                await Task.Run(() => Set.Remove(entity), cancellationToken);
                
                _logger.LogDebug("Applied hard delete for {EntityType} ID {EntityId}", entityType, entityId);
            }
            
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully performed {DeleteType} delete for {EntityType} with ID {EntityId} in {Duration}ms", 
                deleteType, entityType, entityId, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to delete {EntityType} with ID {EntityId} after {Duration}ms", 
                entityType, entityId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        
        _logger.LogDebug("Starting GetAllIncludeDeleted operation for {EntityType}", entityType);
        
        try
        {
            var entities = await Set.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {Count} {EntityType} entities (including deleted) in {Duration}ms", 
                entities.Count, entityType, stopwatch.ElapsedMilliseconds);
            
            // Performance warning for large datasets
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: GetAllIncludeDeleted for {EntityType} took {Duration}ms for {Count} entities (threshold: 1000ms)", 
                    entityType, stopwatch.ElapsedMilliseconds, entities.Count);
            }
            
            return entities;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve {EntityType} entities (including deleted) after {Duration}ms", 
                entityType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}