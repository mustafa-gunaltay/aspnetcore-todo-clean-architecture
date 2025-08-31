using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models;

namespace TodoBackend.Domain.Interfaces;

public interface ICategoryRepository : BuildingBlocks.IRepository<Category>
{
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetCategoriesWithTaskCountAsync(CancellationToken ct = default);
    Task<bool> HasActiveTasksAsync(int categoryId, CancellationToken ct = default);
}
