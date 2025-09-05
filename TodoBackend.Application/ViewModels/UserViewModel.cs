namespace TodoBackend.Application.ViewModels;

public class UserViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int TaskCount { get; set; } // Kullan?c?n?n toplam task say?s?
}