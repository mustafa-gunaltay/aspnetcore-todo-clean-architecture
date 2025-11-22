using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models.BuildingBlocks;
using TodoBackend.Domain.SpecificationConfig;
using System.Diagnostics;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class ReadOnlyRepository<TEntity> : RepositoryProperties<TEntity>, IReadOnlyRepository<TEntity> where TEntity : Entity
{
    private readonly ILogger<ReadOnlyRepository<TEntity>> _logger;
    
    public ReadOnlyRepository(TodoBackendDbContext dbContext, ILogger<ReadOnlyRepository<TEntity>> logger) : base(dbContext) 
    { 
        _logger = logger;
    }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        
        _logger.LogDebug("Starting GetById operation for {EntityType} with ID {EntityId}", entityType, id);
        
        try
        {
            var entity = await SetAsNoTracking.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
            stopwatch.Stop();
            
            if (entity != null)
            {
                _logger.LogInformation("Successfully retrieved {EntityType} with ID {EntityId} in {Duration}ms", 
                    entityType, id, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("{EntityType} with ID {EntityId} not found in {Duration}ms", 
                    entityType, id, stopwatch.ElapsedMilliseconds);
            }
            
            return entity;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve {EntityType} with ID {EntityId} after {Duration}ms", 
                entityType, id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        
        _logger.LogDebug("Starting GetAll operation for {EntityType}", entityType);
        
        try
        {
            var entities = await SetAsNoTracking.ToListAsync(cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {Count} {EntityType} entities in {Duration}ms", 
                entities.Count, entityType, stopwatch.ElapsedMilliseconds);
            
            // Performance warning for large datasets
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: GetAll for {EntityType} took {Duration}ms for {Count} entities (threshold: 1000ms)", 
                    entityType, stopwatch.ElapsedMilliseconds, entities.Count);
            }
            
            return entities;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to retrieve {EntityType} entities after {Duration}ms", 
                entityType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<(int TotalCount, IReadOnlyList<TEntity> Data)> ListAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var entityType = typeof(TEntity).Name;
        
        _logger.LogDebug("Starting ListAsync operation with Specification for {EntityType}", entityType);
        
        try
        {
            var query = SetAsNoTracking.Specify(specification);

            var totalCount = 0;

            if (specification.IsPagingEnabled)
            {
                totalCount = await query.CountAsync(cancellationToken);
                query = query.Skip(specification.Skip).Take(specification.Take);
                
                _logger.LogDebug("Pagination enabled for {EntityType}: PageSize={PageSize}, Skip={Skip}, Take={Take}, TotalCount={TotalCount}",
                    entityType, specification.Take, specification.Skip, specification.Take, totalCount);
            }
            
            var data = await query.ToListAsync(cancellationToken);
            stopwatch.Stop();
            
            _logger.LogInformation("Successfully retrieved {Count} {EntityType} entities with Specification in {Duration}ms (TotalCount: {TotalCount})", 
                data.Count, entityType, stopwatch.ElapsedMilliseconds, totalCount);
            
            // Performance warning for slow queries
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow query detected: ListAsync for {EntityType} took {Duration}ms for {Count} entities (threshold: 1000ms)", 
                    entityType, stopwatch.ElapsedMilliseconds, data.Count);
            }

            return (totalCount, data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to execute ListAsync for {EntityType} after {Duration}ms", 
                entityType, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
