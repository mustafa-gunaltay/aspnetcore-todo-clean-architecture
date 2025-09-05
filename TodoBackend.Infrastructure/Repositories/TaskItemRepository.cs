using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class TaskItemRepository : Repository<TaskItem>, ITaskItemRepository
{
    public TaskItemRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser) 
        : base(dbContext, currentUser)
    {
    }

    public Task<bool> DeleteUserFromTaskAsync(int taskItemId, string deletedBy, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetFilteredAsync(int userId, bool? isCompleted = null, Priority? priority = null, DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItem>> GetUpcomingTasksAsync(int userId, int days = 7, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
