using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Inside;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository 
{
    public CategoryRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser, ILogger<Repository<Category>> logger) : 
        base(dbContext, currentUser, logger)
    {
    }

    // New method to get category with user information
    public async Task<Category?> GetByIdWithUserAsync(int id, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(c => c.User) // User bilgisini de getir
            .Include(c => c.TaskItemCategories) // TaskItemCategories bilgisini de getir
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null, CancellationToken ct = default)
    {
        // Firstly IgnoreQueryFilters in order to find if there is a name of soft deleted category in DB
        var query = Set.IgnoreQueryFilters().AsNoTracking().Where(c => c.Name == name);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<bool> IsNameUniqueForUserAsync(string name, int userId, int? excludeId = null, CancellationToken ct = default)
    {
        // Check uniqueness within the same user's categories (including soft deleted ones)
        var query = Set.IgnoreQueryFilters().AsNoTracking()
            .Where(c => c.Name == name && c.UserId == userId);
        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);
        return !await query.AnyAsync(ct);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(c => c.User) // User bilgisini de getir
            .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted, ct);
    }

    public async Task<Category?> GetByNameForUserAsync(string name, int userId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(c => c.User) // User bilgisini de getir
            .FirstOrDefaultAsync(c => c.Name == name && c.UserId == userId && !c.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesWithTaskCountAsync(CancellationToken ct = default)
    {
        // TaskItemCategories ve User ile birlikte getir
        return await Set.AsNoTracking()
            .Include(c => c.TaskItemCategories)
            .Include(c => c.User) // User bilgisini de getir
            .Where(c => !c.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesForUserAsync(int userId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(c => c.TaskItemCategories)
            .Include(c => c.User) // User bilgisini de getir
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<bool> HasActiveTasksAsync(int categoryId, CancellationToken ct = default)
    {
        // Category'nin aktif (IsDeleted=false) TaskItemCategory ilişkisi var mı?
        var category = await Set.AsNoTracking()
            .Include(c => c.TaskItemCategories) // Category ile ilişkili TaskItemCategories navigation property'sini getirir
            .FirstOrDefaultAsync(c => c.Id == categoryId && !c.IsDeleted, ct); // Veritabanında Id'si verilen ve silinmemiş (IsDeleted == false) ilk Category kaydını getirir.
        if (category == null)
            return false;
        return category.TaskItemCategories.Any(tc => !tc.IsDeleted && tc.TaskItem != null && !tc.TaskItem.IsDeleted && !tc.TaskItem.IsCompleted);
    }
}
