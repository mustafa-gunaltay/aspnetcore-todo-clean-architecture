namespace TodoBackend.Application.ViewModels;

public class CategoryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int TaskCount { get; set; }
    public UserSummaryViewModel? User { get; set; } // Category'nin ait oldu?u user bilgileri
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// User özet bilgileri için ViewModel
public class UserSummaryViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}
