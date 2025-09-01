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
    public string Password { get; private set; } = string.Empty; // NOT NULL, private setter

    // Navigation property - 1 to Many
    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

    // Parametresiz constructor - EF Core için
    public User()
    {
        Email = string.Empty;
        Password = string.Empty;
    }

    // İş kurallarını zorlayan constructor
    public User(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password is required.");

        Email = email.Trim();
        Password = password;
    }

    public void ChangeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        Email = email.Trim();
    }

    public void ChangePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new DomainException("Password is required.");
        Password = password;
    }
}
