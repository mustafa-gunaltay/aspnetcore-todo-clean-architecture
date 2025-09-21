using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class UnitOfWork
{
    private readonly TodoBackendDbContext _dbcontext;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(TodoBackendDbContext dbcontext, ILogger<UnitOfWork> logger)
    {
        _dbcontext = dbcontext;
        _logger = logger;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogDebug("Starting SaveChanges operation");
        
        try
        {
            var result = await _dbcontext.SaveChangesAsync(cancellationToken);
            stopwatch.Stop();
            
            var success = result > 0;
            
            if (success)
            {
                _logger.LogInformation("Successfully saved {ChangeCount} changes to database in {Duration}ms", 
                    result, stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _logger.LogDebug("No changes detected to save in {Duration}ms", stopwatch.ElapsedMilliseconds);
            }
            
            // Performance warning for slow database operations
            if (stopwatch.ElapsedMilliseconds > 2000)
            {
                _logger.LogWarning("Slow database operation detected: SaveChanges took {Duration}ms for {ChangeCount} changes (threshold: 2000ms)", 
                    stopwatch.ElapsedMilliseconds, result);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Failed to save changes to database after {Duration}ms", 
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
