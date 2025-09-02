using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity> where TEntity : Entity
{
    private readonly User _currentUser;
    public Repository(TodoBackendDbContext dbContext, User currentUser) : base(dbContext)
    {
        _currentUser = currentUser;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable)
        {
            trackable.Created(_currentUser.Email);
        }
        await Set.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable)
        {
            trackable.Updated(_currentUser.Email);
        }
        await Task.Run(() => Set.Update(entity), cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable)
        {
            trackable.Deleted(_currentUser.Email);
            await Task.Run(() => Set.Update(entity), cancellationToken);
        }
        else
        {
            await Task.Run(() => Set.Remove(entity), cancellationToken);
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await Set.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
    }
}