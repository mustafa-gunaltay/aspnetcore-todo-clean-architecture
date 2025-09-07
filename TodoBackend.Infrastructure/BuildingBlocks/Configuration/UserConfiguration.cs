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
            .HasMaxLength(256) // NVARCHAR(256) - DB ?emas?na uygun
            .HasColumnType("NVARCHAR(256)");

        builder.Property(u => u.Password)
            .IsRequired()
            .HasMaxLength(200) // NVARCHAR(200) - DB ?emas?na uygun
            .HasColumnType("NVARCHAR(200)")
            .HasColumnName("Password"); // [Password] - reserved word

        // Audit fields - DB ?emas?na uygun
        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("DATETIME2(0)");

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("NVARCHAR(100)");

        builder.Property(u => u.UpdatedAt)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(u => u.UpdatedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasColumnType("NVARCHAR(100)");

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false) // CONSTRAINT DF_User_IsDeleted DEFAULT (0)
            .HasColumnType("BIT");

        builder.Property(u => u.DeletedAt)
            .IsRequired(false)
            .HasColumnType("DATETIME2(0)");

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(100)
            .IsRequired(false)
            .HasColumnType("NVARCHAR(100)");

        // Unique constraint for Email - UQ_User_Email
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UQ_User_Email");

        // Navigation: One-to-many with TaskItem
        builder
            .HasMany(u => u.TaskItems)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.NoAction); // ON DELETE NO ACTION

        // Soft delete için global query filter
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}