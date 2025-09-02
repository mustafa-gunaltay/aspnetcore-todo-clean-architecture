using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Interfaces.BuildingBlocks;
using TodoBackend.Domain.Models.BuildingBlocks;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class ReadOnlyRepository<TEntity> : RepositoryProperties<TEntity>, IReadOnlyRepository<TEntity> where TEntity : Entity
{
    public ReadOnlyRepository(TodoBackendDbContext dbContext) : base(dbContext) { }

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await SetAsNoTracking.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await SetAsNoTracking.ToListAsync(cancellationToken);
    }
}
