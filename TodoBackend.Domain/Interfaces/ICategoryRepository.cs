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
    Task<bool> IsNameUniqueForUserAsync(string name, int userId, int? excludeId = null, CancellationToken ct = default);
    Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Category?> GetByNameForUserAsync(string name, int userId, CancellationToken ct = default);
    Task<Category?> GetByIdWithUserAsync(int id, CancellationToken ct = default); // New method to get category with user info
    Task<IReadOnlyList<Category>> GetCategoriesWithTaskCountAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetCategoriesForUserAsync(int userId, CancellationToken ct = default);
    Task<bool> HasActiveTasksAsync(int categoryId, CancellationToken ct = default);
}
