using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Interfaces.Inside;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class TaskItemCategoryRepository : Repository<TaskItemCategory>, ITaskItemCategoryRepository
{
    public TaskItemCategoryRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser, ILogger<Repository<TaskItemCategory>> logger) 
        : base(dbContext, currentUser, logger)
    {
    }

    // YENİ: Gereksinim 8 - Görevler kategorilere bağlanabilmelidir
    public async Task<bool> AssignTaskToCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default)
    {
        
        // Zaten atanmış mı kontrol et 
        var existingRelation = Set.IgnoreQueryFilters().AsNoTracking()
            .FirstOrDefault(tc => tc.TaskItemId == taskItemId && tc.CategoryId == categoryId);
        if (existingRelation != null && !existingRelation.IsDeleted)
            return false; // Zaten atanmış

        // Soft deleted relation varsa restore et
        if (existingRelation != null && existingRelation.IsDeleted)
        {
            existingRelation.Restore();
            await UpdateAsync(existingRelation, ct);
            return true;
        }

        // Yeni relation oluştur
        var newRelation = new TaskItemCategory(taskItemId, categoryId);
        await AddAsync(newRelation, ct);
        return true;
    }

    // YENİ: Gereksinim 9 - Görevler kategorilerden ayrılabilmelidir
    public async Task<bool> RemoveTaskFromCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default)
    {
        var relation = await GetByTaskAndCategoryAsync(taskItemId, categoryId, ct);
        if (relation == null || relation.IsDeleted)
            return false;

        // Soft delete
        await DeleteAsync(relation, ct);
        return true;
    }

    public async Task<bool> IsTaskAssignedToCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default)
    {
        var relation = await GetByTaskAndCategoryAsync(taskItemId, categoryId, ct);
        return relation != null && !relation.IsDeleted;
    }

    public async Task<IReadOnlyList<TaskItemCategory>> GetActiveRelationsByTaskIdAsync(int taskItemId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Where(tc => tc.TaskItemId == taskItemId && !tc.IsDeleted)
            .Include(tc => tc.Category)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItemCategory>> GetCategoriesByTaskItemIdAsync(int taskItemId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Where(tc => tc.TaskItemId == taskItemId && !tc.IsDeleted)
            .Include(tc => tc.Category)
                .ThenInclude(c => c.User) // User bilgisini de dahil et
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItemCategory>> GetTaskItemsByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Where(tc => tc.CategoryId == categoryId && !tc.IsDeleted)
            .Include(tc => tc.TaskItem)
                .ThenInclude(ti => ti.User) // TaskItem'ın User bilgisini include et (UserEmail için)
                    .ThenInclude(c => c.Categories)
            .ToListAsync(ct);
    }

    public async Task<TaskItemCategory?> GetByTaskAndCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.TaskItemId == taskItemId && tc.CategoryId == categoryId, ct);
    }

    public async Task<IReadOnlyList<TaskItemCategory>> GetByTaskItemIdsAsync(IEnumerable<int> taskItemIds, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Where(tc => taskItemIds.Contains(tc.TaskItemId) && !tc.IsDeleted)
            .Include(tc => tc.Category)
            .Include(tc => tc.TaskItem)
            .ToListAsync(ct);
    }

    public async Task DeleteAllByTaskItemIdAsync(int taskItemId, CancellationToken ct = default)
    {
        var relations = await Set
            .Where(tc => tc.TaskItemId == taskItemId && !tc.IsDeleted)
            .ToListAsync(ct);

        foreach (var relation in relations)
        {
            await DeleteAsync(relation, ct);
        }
    }

    public async Task DeleteAllByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        var relations = await Set
            .Where(tc => tc.CategoryId == categoryId && !tc.IsDeleted)
            .ToListAsync(ct);

        foreach (var relation in relations)
        {
            await DeleteAsync(relation, ct);
        }
    }
}
