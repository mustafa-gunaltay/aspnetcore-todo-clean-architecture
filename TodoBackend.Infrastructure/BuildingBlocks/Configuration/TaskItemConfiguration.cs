using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoBackend.Domain.Models;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Infrastructure.BuildingBlocks.Configuration;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Table name
        builder.ToTable("TaskItem");

        // Primary key
        builder.HasKey(t => t.Id);

        // Properties
        builder.Property(t => t.Id)
            .IsRequired()
            .ValueGeneratedOnAdd(); // IDENTITY(1,1)

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200) // NVARCHAR(200)
            .HasColumnType("NVARCHAR(200)")
            .HasColumnName("Title");

        builder.Property(t => t.Description)
            .HasColumnType("NVARCHAR(MAX)") // NVARCHAR(MAX) - DB ?emas?na uygun
            .IsRequired(false)
            .HasColumnName("Description"); // [Description] - reserved word

        builder.Property(t => t.Priority)
            .IsRequired()
            .HasColumnType("TINYINT") // TINYINT - DB ?emas?na uygun
            .HasConversion<byte>()
            .HasColumnName("Priority"); // [Priority] - reserved word

        builder.Property(t => t.DueDate)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(t => t.CompletedAt)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(t => t.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false)
            .HasColumnType("BIT");

        builder.Property(t => t.UserId)
            .IsRequired(false); // NULL - User silinebilir

        // Audit fields - DB ?emas?na uygun
        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnType("DATETIME2(0)");

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("NVARCHAR(100)");

        builder.Property(t => t.UpdatedAt)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasColumnType("NVARCHAR(100)");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false) // CONSTRAINT DF_TaskItem_IsDeleted DEFAULT (0)
            .HasColumnType("BIT");

        builder.Property(t => t.DeletedAt)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasColumnType("NVARCHAR(100)");

        // Relationships
        builder
            .HasOne(t => t.User)
            .WithMany(u => u.TaskItems)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.NoAction) // ON DELETE NO ACTION
            .HasConstraintName("FK_TaskItem_User");

        builder
            .HasMany(t => t.TaskItemCategories)
            .WithOne(tc => tc.TaskItem)
            .HasForeignKey(tc => tc.TaskItemId)
            .OnDelete(DeleteBehavior.NoAction); // Defensive programming

        // Soft delete için global query filter
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}