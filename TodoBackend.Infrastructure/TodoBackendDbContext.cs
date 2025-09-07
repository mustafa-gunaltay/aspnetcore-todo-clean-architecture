using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TodoBackend.Domain.Models;

namespace TodoBackend.Infrastructure;

public class TodoBackendDbContext : DbContext
{
    public DbSet<Category> categories { get; set; }
    public DbSet<TaskItem> taskItems { get; set; }
    public DbSet<User> users { get; set; }
    public DbSet<TaskItemCategory> taskItemCategories { get; set; }

    public TodoBackendDbContext(DbContextOptions<TodoBackendDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tüm IEntityTypeConfiguration<> sınıflarını otomatik uygula
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
