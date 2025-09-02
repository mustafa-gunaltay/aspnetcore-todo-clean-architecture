using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class RepositoryProperties<TEntity> where TEntity : Entity
{
    protected readonly TodoBackendDbContext _dbContext;

    public RepositoryProperties(TodoBackendDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected DbSet<TEntity> Set => _dbContext.Set<TEntity>();

    protected IQueryable<TEntity> SetAsNoTracking
    {
        get
        {
            var query = Set.AsNoTracking();
            // Soft delete filtrelemesi: AuditableEntity'den türeyenler için
            if (typeof(AuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                query = query.Where(e => !(e as AuditableEntity)!.IsDeleted);
            }
            return query;
        }
    }
}
