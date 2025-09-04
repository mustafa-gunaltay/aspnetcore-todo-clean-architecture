using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models.BuildingBlocks;
using TodoBackend.Domain.Interfaces;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class Repository<TEntity> : ReadOnlyRepository<TEntity>, IRepository<TEntity> where TEntity : Entity
{
    private readonly ICurrentUser _currentUser;
    public Repository(TodoBackendDbContext dbContext, ICurrentUser currentUser) : base(dbContext)
    {
        _currentUser = currentUser;
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable)
        {
            trackable.Created(_currentUser.UserName);
        }
        await Set.AddAsync(entity, cancellationToken);
    }

    public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable)
        {
            trackable.Updated(_currentUser.UserName);
        }
        await Task.Run(() => Set.Update(entity), cancellationToken);
    }

    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        if (entity is AuditableEntity trackable) // "entity is-a AuditableEntity ?" kontrolu yapilir, oyleyse entity -> trackable'a downcast edilir (javadaki instanceof operatoru)
        {
            // AuditableEntity ise soft delete (mantıksal silme) yapar (isDeleted=true)
            trackable.Deleted(_currentUser.UserName);
            await Task.Run(() => Set.Update(entity), cancellationToken);
        }
        else
        {
            // AuditableEntity değilse hard delete (fiziksel silme) yapar
            await Task.Run(() => Set.Remove(entity), cancellationToken); 
        }
    }

    public async Task<IReadOnlyList<TEntity>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default)
    {
        return await Set.IgnoreQueryFilters().AsNoTracking().ToListAsync(cancellationToken);
    }
}