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
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired(false);

        builder.Property(t => t.Priority)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .IsRequired(false);

        builder.Property(t => t.CompletedAt)
            .IsRequired(false);

        builder.Property(t => t.IsCompleted)
            .IsRequired();

        builder.Property(t => t.UserId)
            .IsRequired(false);

        // Audit fields - DB ?emas?na uygun
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);

        builder.Property(t => t.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.DeletedAt)
            .IsRequired(false);

        builder.Property(t => t.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Relationships
        builder
            .HasOne(t => t.User)
            .WithMany(u => u.TaskItems)
            .HasForeignKey(t => t.UserId);

        builder
            .HasMany(t => t.TaskItemCategories)
            .WithOne(tc => tc.TaskItem)
            .HasForeignKey(tc => tc.TaskItemId);

        // Soft delete için global query filter
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}