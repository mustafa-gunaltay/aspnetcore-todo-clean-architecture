using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.DomainExceptions;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Domain.Models;

public class User : BuildingBlocks.AuditableEntity
{
    public string Email { get; private set; } = string.Empty; // NOT NULL, private setter
    public string PasswordHash { get; private set; } = string.Empty; // NOT NULL, private setter
    public string PasswordSalt { get; private set; } = string.Empty; // NOT NULL, private setter

    // Navigation properties - 1 to Many
    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();

    // Parametresiz constructor - EF Core için
    private User()
    {
        Email = string.Empty;
        PasswordHash = string.Empty;
        PasswordSalt = string.Empty;
    }

    // İş kurallarını zorlayan private constructor
    private User(string email, string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        
        Email = email.Trim();
        SetPasswordDigest(hash, salt);
    }

    /// <summary>
    /// Factory method - User oluşturur (hash ve salt zaten hazır olmalı)
    /// </summary>
    public static User Create(string email, string hash, string salt) =>
        new User(email, hash, salt);

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        Email = email.Trim();
    }

    /// <summary>
    /// Yeni password hash ve salt uygular (hash ve salt zaten hazır olmalı)
    /// </summary>
    public void ApplyNewPassword(string hash, string salt) =>
        SetPasswordDigest(hash, salt);

    /// <summary>
    /// Password digest (hash + salt) günceller - private method
    /// </summary>
    private void SetPasswordDigest(string hash, string salt)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash is required.");
        if (string.IsNullOrWhiteSpace(salt))
            throw new DomainException("Password salt is required.");
        
        PasswordHash = hash;
        PasswordSalt = salt;
    }
}
