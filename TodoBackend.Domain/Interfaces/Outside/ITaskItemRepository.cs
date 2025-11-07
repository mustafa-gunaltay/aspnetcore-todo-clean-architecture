using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Models;

namespace TodoBackend.Domain.Interfaces.Out;

public interface ITaskItemRepository : BuildingBlocks.IRepository<TaskItem>
{
    // User'a göre filtreleme (temel gereksinim)
    Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(int userId, CancellationToken ct = default);

    // Filtreleme (gereksinimlerden)
    Task<IReadOnlyList<TaskItem>> GetFilteredAsync(
        int userId,
        bool? isCompleted = null,
        Priority? priority = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? categoryId = null,
        CancellationToken ct = default);


    // High priority görevler için DueDate kontrolü
    Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(int userId, CancellationToken ct = default);
    Task<IReadOnlyList<TaskItem>> GetUpcomingTasksAsync(int userId, int days = 7, CancellationToken ct = default);

    // YENİ: User ile TaskItem arasındaki ilişkiyi kesme (User'in bir TaskItem'i (gorevi) yapmaktan vazgecince silebilmesi icin)
    Task<bool> DeleteUserFromTaskAsync(int taskItemId, CancellationToken ct = default);
    
    // YENİ: User silindiğinde o user'ın tüm task'larını soft delete et
    Task<int> SoftDeleteAllTasksByUserIdAsync(int userId, CancellationToken ct = default);
    
    // YENİ: Get task by ID with User and Categories included
    Task<TaskItem?> GetByIdWithDetailsAsync(int id, CancellationToken ct = default);
}
