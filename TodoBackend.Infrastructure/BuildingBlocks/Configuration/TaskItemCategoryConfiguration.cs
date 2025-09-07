using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoBackend.Domain.Models;

namespace TodoBackend.Infrastructure.BuildingBlocks.Configuration;

public class TaskItemCategoryConfiguration : IEntityTypeConfiguration<TaskItemCategory>
{
    public void Configure(EntityTypeBuilder<TaskItemCategory> builder)
    {
        // Table name - DB'de TaskItem_Category
        builder.ToTable("TaskItem_Category");

        // ÖNEML?: DB'de composite primary key var ama domain'de Id var
        // Domain model Entity'den inherit oldu?u için Id property'si var
        // Ama DB'de composite key: (TaskItemId, CategoryId)
        
        // Entity Id'sini ignore et ve composite key kullan
        builder.Ignore(tc => tc.Id);
        
        // Composite primary key - DB ?emas?na uygun
        builder.HasKey(tc => new { tc.TaskItemId, tc.CategoryId });

        // Properties
        builder.Property(tc => tc.TaskItemId)
            .IsRequired();

        builder.Property(tc => tc.CategoryId)
            .IsRequired();

        // Audit fields - DB ?emas?na uygun
        builder.Property(tc => tc.CreatedAt)
            .IsRequired();

        builder.Property(tc => tc.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(tc => tc.UpdatedAt)
            .IsRequired(false);

        builder.Property(tc => tc.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(tc => tc.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(tc => tc.DeletedAt)
            .IsRequired(false);

        builder.Property(tc => tc.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Relationships - DB foreign key constraints'e uygun
        builder
            .HasOne(tc => tc.TaskItem)
            .WithMany(t => t.TaskItemCategories)
            .HasForeignKey(tc => tc.TaskItemId);

        builder
            .HasOne(tc => tc.Category)
            .WithMany(c => c.TaskItemCategories)
            .HasForeignKey(tc => tc.CategoryId);

        // Soft delete için global query filter
        builder.HasQueryFilter(tc => !tc.IsDeleted);
    }
}