using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TodoBackend.Domain.Models;

namespace TodoBackend.Infrastructure.BuildingBlocks.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name - DB'de [User]
        builder.ToTable("User");

        // Primary key
        builder.HasKey(u => u.Id);

        // Properties
        builder.Property(u => u.Id)
            .IsRequired()
            .ValueGeneratedOnAdd(); // IDENTITY(1,1)

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256); // NVARCHAR(256) - DB semasina uygun


        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(200);

        // Audit fields - DB semasina uygun
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false);

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.DeletedAt)
            .IsRequired(false);

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false);

        // Unique constraint for Email
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Navigation: One-to-many with TaskItem
        builder
            .HasMany(u => u.TaskItems)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);

        // Soft delete için global query filter
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}