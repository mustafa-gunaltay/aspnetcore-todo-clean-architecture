using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.DomainExceptions;


namespace TodoBackend.Domain.Models;


public class TaskItem : BuildingBlocks.AuditableEntity
{
    public string Title { get; private set; } = string.Empty; // NOT NULL, private setter
    public string? Description { get; private set; } // Nullable - DB'de NULL olabilir
    public Priority Priority { get; private set; } // NOT NULL, TINYINT
    public DateTime? DueDate { get; private set; } // Nullable
    public DateTime? CompletedAt { get; private set; } // Nullable
    public bool IsCompleted { get; private set; } = false; // NOT NULL, default false
    public int? UserId { get; private set; } // Nullable, FK

    // Navigation properties
    public User? User { get; set; }
    public ICollection<TaskItemCategory> TaskItemCategories { get; set; } = new List<TaskItemCategory>();

    // Parametresiz constructor - EF Core için
    public TaskItem()
    {
        Title = string.Empty;
        Priority = Priority.Medium;
    }

    // İş kurallarını zorlayan constructor
    public TaskItem(string title, string? description, Priority priority, DateTime? dueDate, int userId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        if (userId <= 0)
            throw new DomainException("Valid UserId is required.");
        if (priority == Priority.High && dueDate is null)
            throw new DomainException("High priority tasks require a due date.");

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        DueDate = dueDate;
        UserId = userId;
        IsCompleted = false;
        CompletedAt = null;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title is required.");
        Title = title.Trim();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void SetPriority(Priority priority)
    {
        if (priority == Priority.High && DueDate is null)
            throw new DomainException("High priority tasks require a due date.");
        Priority = priority;
    }

    public void SetDueDate(DateTime? dueDate)
    {
        if (Priority == Priority.High && dueDate is null)
            throw new DomainException("High priority tasks require a due date.");
        DueDate = dueDate;
    }

    public void Complete()
    {
        if (IsCompleted)
            return;

        var nowDate = DateTime.Now.Date;
        if (DueDate.HasValue && DueDate.Value.Date < nowDate)
            throw new DomainException("A task with a past due date cannot be completed.");

        CompletedAt = DateTime.Now;
        IsCompleted = true;
    }

    public void Reopen()
    {
        if (!IsCompleted)
            return;
        IsCompleted = false;
        CompletedAt = null;
    }

    public void AssignToCategory(Category category)
    {
        if (category is null)
            throw new DomainException("Category is required.");
        if (category.IsDeleted)
            throw new DomainException("Cannot assign task to a deleted category.");

        // Zaten varsa ekleme
        if (TaskItemCategories.Any(tc => tc.CategoryId == category.Id && !tc.IsDeleted))
            return;

        var taskItemCategory = new TaskItemCategory(this.Id, category.Id);
        TaskItemCategories.Add(taskItemCategory);
    }

    public void RemoveFromCategory(int categoryId)
    {
        var existingRelation = TaskItemCategories
            .FirstOrDefault(tc => tc.CategoryId == categoryId && !tc.IsDeleted);

        if (existingRelation != null)
        {
            existingRelation.SoftDelete();
        }
    }

    // Yeni method: Task'ın User ile ilisigini kes ve soft-delete et
    // UserId null yapildiginda TaskItem'in da sistemde olmasinin bir anlami kalmiyor dolayisiyla UserId=null ile birlikte TaskItem soft delete yapiliyor
    public void SoftDeleteWithUser(string deletedBy)
    {
        SoftDelete(); // Base class'tan
        Deleted(deletedBy); // Audit için
        UserId = null; // User referansını kaldır
    }

    // Task restore edildiğinde UserId'nin tekrar atanması gerekir
    public void RestoreWithUser(int userId)
    {
        if (userId <= 0)
            throw new DomainException("Valid UserId is required for restore.");

        Restore(); // Base class'tan
        UserId = userId;
    }

    
}
