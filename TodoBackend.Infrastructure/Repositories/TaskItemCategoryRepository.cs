using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;

namespace TodoBackend.Infrastructure.Repositories;

public class TaskItemCategoryRepository : Repository<TaskItemCategory>, ITaskItemCategoryRepository
{
    public TaskItemCategoryRepository(TodoBackendDbContext dbContext, ICurrentUser currentUser) 
        : base(dbContext, currentUser)
    {
    }

    public Task DeleteAllByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAllByTaskItemIdAsync(int taskItemId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItemCategory>> GetTaskItemsByCategoryIdAsync(int categoryId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<TaskItemCategory?> GetByTaskAndCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItemCategory>> GetCategoriesByTaskItemIdAsync(int taskItemId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TaskItemCategory>> GetByTaskItemIdsAsync(IEnumerable<int> taskItemIds, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
