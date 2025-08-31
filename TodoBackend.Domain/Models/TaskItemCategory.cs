using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.DomainExceptions;

namespace TodoBackend.Domain.Models;

public class TaskItemCategory : BuildingBlocks.AuiditableEntity
{
    public int TaskItemId { get; private set; } // NOT NULL, FK
    public int CategoryId { get; private set; } // NOT NULL, FK

    // Navigation properties
    public TaskItem TaskItem { get; set; } = null!;
    public Category Category { get; set; } = null!;

    // Parametresiz constructor - EF Core için
    public TaskItemCategory()
    {
    }

    // İş kurallarını zorlayan constructor
    public TaskItemCategory(int taskItemId, int categoryId)
    {
        if (taskItemId <= 0)
            throw new DomainException("TaskItemId must be greater than zero.");
        if (categoryId <= 0)
            throw new DomainException("CategoryId must be greater than zero.");

        TaskItemId = taskItemId;
        CategoryId = categoryId;
    }

    public TaskItemCategory(TaskItem taskItem, Category category)
    {
        TaskItem = taskItem ?? throw new DomainException("TaskItem cannot be null.");
        Category = category ?? throw new DomainException("Category cannot be null.");
        TaskItemId = taskItem.Id;
        CategoryId = category.Id;
    }
}
