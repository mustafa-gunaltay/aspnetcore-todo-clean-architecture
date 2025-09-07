using TodoBackend.Domain.Enums;

namespace TodoBackend.Application.ViewModels;

public class TaskItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Priority Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; } // Navigation property'den gelecek
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Category bilgileri
    public IList<CategorySummaryViewModel> Categories { get; set; } = new List<CategorySummaryViewModel>();
}

// TaskItem içinde kullan?lacak basit category bilgisi
public class CategorySummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}