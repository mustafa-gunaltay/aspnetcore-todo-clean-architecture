using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Interfaces.Inside;
using TodoBackend.Domain.Interfaces.Out;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class TaskItemRepository : Repository<TaskItem>, ITaskItemRepository
{
    private readonly ICurrentUser _currentUser;

    public TaskItemRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser, ILogger<Repository<TaskItem>> logger) 
        : base(dbContext, currentUser, logger)
    {
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await Set.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskItemCategories)
                .ThenInclude(tc => tc.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetFilteredAsync(
        int userId, 
        bool? isCompleted = null, 
        Priority? priority = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null, 
        int? categoryId = null, 
        CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskItemCategories)
                .ThenInclude(tc => tc.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // IsCompleted filtresi
        if (isCompleted.HasValue)
        {
            query = query.Where(t => t.IsCompleted == isCompleted.Value);
        }

        // Priority filtresi
        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        // Tarih aralığı filtresi (CreatedAt veya DueDate'e göre)
        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt.Date >= startDate.Value.Date || 
                                   (t.DueDate.HasValue && t.DueDate.Value.Date >= startDate.Value.Date));
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt.Date <= endDate.Value.Date || 
                                   (t.DueDate.HasValue && t.DueDate.Value.Date <= endDate.Value.Date));
        }

        // Category filtresi (TaskItemCategory junction table üzerinden)
        if (categoryId.HasValue)
        {
            query = query.Where(t => t.TaskItemCategories.Any(tc => 
                tc.CategoryId == categoryId.Value && !tc.IsDeleted));
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(int userId, CancellationToken ct = default)
    {
        var today = DateTime.Now;
        
        return await Set.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskItemCategories)
                .ThenInclude(tc => tc.Category)
            .Where(t => t.UserId == userId && 
                       !t.IsDeleted && 
                       !t.IsCompleted && 
                       t.DueDate.HasValue && 
                       t.DueDate.Value.Date < today)
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TaskItem>> GetUpcomingTasksAsync(int userId, int days = 7, CancellationToken ct = default)
    {
        var today = DateTime.Now;
        var futureDate = today.AddDays(days);
        
        return await Set.AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskItemCategories)
                .ThenInclude(tc => tc.Category)
            .Where(t => t.UserId == userId && 
                       !t.IsDeleted && 
                       !t.IsCompleted && 
                       t.DueDate.HasValue && 
                       t.DueDate.Value.Date >= today && 
                       t.DueDate.Value.Date <= futureDate)
            .OrderBy(t => t.DueDate)
            .ToListAsync(ct);
    }

    public async Task<bool> DeleteUserFromTaskAsync(int taskItemId, CancellationToken ct = default)
    {
        var taskItem = await GetByIdAsync(taskItemId, ct);
        if (taskItem == null || taskItem.IsDeleted)
            return false;

        // TaskItem domain metodunu kullan
        taskItem.SoftDeleteWithUser(_currentUser.UserName);
        
        await UpdateAsync(taskItem, ct);
        return true;
    }

    public async Task<int> SoftDeleteAllTasksByUserIdAsync(int userId, CancellationToken ct = default)
    {
        // Tracking enabled çünkü update yapacağız
        var userTasks = await Set
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .ToListAsync(ct);

        foreach (var task in userTasks)
        {
            task.SoftDeleteWithUser(_currentUser.UserName);
        }

        // EF Core change tracking sayesinde SaveChanges'de otomatik update olacak
        return userTasks.Count;
    }
}
