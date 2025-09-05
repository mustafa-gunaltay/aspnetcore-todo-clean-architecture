using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models;

namespace TodoBackend.Domain.Interfaces;

public interface ITaskItemCategoryRepository : BuildingBlocks.IRepository<TaskItemCategory>
{
    // TaskItem'a bağlı kategorileri getir
    Task<IReadOnlyList<TaskItemCategory>> GetCategoriesByTaskItemIdAsync(int taskItemId, CancellationToken ct = default);

    // Category'ye bağlı task'ları getir
    Task<IReadOnlyList<TaskItemCategory>> GetTaskItemsByCategoryIdAsync(int categoryId, CancellationToken ct = default);

    // Belirli bir ilişki var mı kontrolü
    Task<TaskItemCategory?> GetByTaskAndCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default);

    // Toplu kategori ataması/çıkarması için
    Task<IReadOnlyList<TaskItemCategory>> GetByTaskItemIdsAsync(IEnumerable<int> taskItemIds, CancellationToken ct = default);

    // Bir task'ın tüm kategori ilişkilerini soft delete
    Task DeleteAllByTaskItemIdAsync(int taskItemId, CancellationToken ct = default);

    // Bir kategorinin tüm task ilişkilerini soft delete
    Task DeleteAllByCategoryIdAsync(int categoryId, CancellationToken ct = default);
}
