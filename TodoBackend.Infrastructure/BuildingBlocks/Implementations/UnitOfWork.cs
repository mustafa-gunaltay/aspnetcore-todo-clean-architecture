using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Infrastructure.BuildingBlocks.Implementations;

public class UnitOfWork
{
    private readonly TodoBackendDbContext _dbcontext;

    public UnitOfWork(TodoBackendDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _dbcontext.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
