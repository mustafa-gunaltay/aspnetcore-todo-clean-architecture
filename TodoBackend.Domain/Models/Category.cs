using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.DomainExceptions;

namespace TodoBackend.Domain.Models;

public class Category : BuildingBlocks.AuditableEntity
{
    public string Name { get; private set; } = string.Empty; // NOT NULL, private setter
    public string Description { get; private set; } = string.Empty; // NOT NULL, private setter

    // Many-to-many navigation - TaskItemCategory üzerinden
    public ICollection<TaskItemCategory> TaskItemCategories { get; set; } = new List<TaskItemCategory>();

    // Parametresiz constructor - EF Core için
    public Category()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    // İş kurallarını zorlayan constructor
    public Category(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Category description is required.");

        Name = name.Trim();
        Description = description.Trim();
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");
        Name = name.Trim();
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Category description is required.");
        Description = description.Trim();
    }
}
