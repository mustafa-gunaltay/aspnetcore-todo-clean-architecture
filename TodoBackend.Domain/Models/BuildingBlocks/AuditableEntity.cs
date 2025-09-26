using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TodoBackend.Domain.Models.BuildingBlocks;

public abstract class AuditableEntity : Entity
{

    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public string UpdatedBy { get; private set; } = string.Empty;
    public bool IsDeleted { get; private set; } = false;
    public DateTime? DeletedAt { get; private set; }
    public string DeletedBy { get; private set; } = string.Empty;

    public void Created(string createdBy)
    {
        CreatedAt = DateTime.Now;
        CreatedBy = createdBy;
    }

    public void Updated(string updatedBy)
    {
        UpdatedAt = DateTime.Now;
        UpdatedBy = updatedBy;
    }

    public void Deleted(string deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.Now;
        DeletedBy = deletedBy;
    }

    public void SoftDelete() => IsDeleted = true;
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = "";
    }
}
