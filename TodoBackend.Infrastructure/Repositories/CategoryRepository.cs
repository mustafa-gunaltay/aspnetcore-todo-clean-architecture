using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository 
{
    public CategoryRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser) : 
        base(dbContext, currentUser)
    {
    }   

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Where(c => c.Name == name && !c.IsDeleted);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await Set.AsNoTracking().FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesWithTaskCountAsync(CancellationToken ct = default)
    {
        // TaskItemCategories ile birlikte getir
        return await Set.AsNoTracking()
            .Include(c => c.TaskItemCategories)
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveTasksAsync(int categoryId, CancellationToken ct = default)
    {
        // Category'nin aktif (IsDeleted=false) TaskItemCategory ilişkisi var mı?
        var category = await Set.AsNoTracking()
            .Include(c => c.TaskItemCategories) // Category ile ilişkili TaskItemCategories navigation property’sini getirir
            .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted, ct); // Veritabanında Id’si verilen ve silinmemiş (IsDeleted == false) ilk Category kaydını getirir.
        if (category == null)
            return false;
        return category.TaskItemCategories.Any(tc => !tc.IsDeleted && tc.TaskItem != null && !tc.TaskItem.IsDeleted && !tc.TaskItem.IsCompleted);
    }
}
