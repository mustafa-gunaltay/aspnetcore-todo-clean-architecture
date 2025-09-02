using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TodoBackend.Infrastructure.BuildingBlocks.Configuration;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Table name
        builder.ToTable("Category");

        // Primary key
        builder.HasKey(c => c.Id);

        // Properties
        builder.Property(c => c.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(400);

        // Audit fields
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DeletedAt)
            .IsRequired(false);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Unique constraint for Name
        builder.HasIndex(c => c.Name)
            .IsUnique();

        // Navigation: Many-to-many with TaskItem via TaskItemCategory
        builder
            .HasMany(c => c.TaskItemCategories)
            .WithOne(tc => tc.Category)
            .HasForeignKey(tc => tc.CategoryId);

        // Soft delete için global query filter (opsiyonel)
        builder.HasQueryFilter(c => !c.IsDeleted);

        //Category tablosundaki IsDeleted alanı 0 olan(silinmemiş) kayıtların otomatik olarak
        //listelenmesini sağlar.
        //IsDeleted = 1 olanlar ise EF Core sorgularında gizlenir.

    }
}
