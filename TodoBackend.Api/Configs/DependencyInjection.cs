using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using TodoBackend.Infrastructure;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Infrastructure.Repositories;
using TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;
using TodoBackend.Domain.Models;
using TodoBackend.Domain.Enums;

namespace TodoBackend.Api.Configs;

public static class DependencyInjection
{
    public static IServiceCollection Register(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterDbContext(configuration)
            .RegisterRepositories()
            .RegisterMediatR()
            //.RegisterValidators()
            .RegisterCurrentUser()
            .RegisterSwagger();

        return services;
    }

    private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TodoBackendDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")),
            ServiceLifetime.Scoped);
        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {


        services.AddScoped<ICategoryRepository, CategoryRepository>();
        // Diğer repository kayıtları
        // services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        // services.AddScoped<IUserRepository, UserRepository>();
        // services.AddScoped<ITaskItemCategoryRepository, TaskItemCategoryRepository>();

        // Mock implementasyonlar - Gecici cozum (bunun yerine bos bir sekilde asagidaki 3 Repository'nin siniflari ve istenen metot impelemtasyonlari bos bir sekilde implemente edilebilir )
        services.AddScoped<ITaskItemRepository, MockTaskItemRepository>();
        services.AddScoped<IUserRepository, MockUserRepository>();
        services.AddScoped<ITaskItemCategoryRepository, MockTaskItemCategoryRepository>();


        services.AddScoped<ITodoBackendUnitOfWork, TodoBackendUnitOfWork>();

        return services;
    }

    private static IServiceCollection RegisterMediatR(this IServiceCollection services)
    {
        // Application katmanındaki handler'ları bulmak için
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCategoryCommandHandler>();
        });
        return services;
    }

    private static IServiceCollection RegisterCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    //private static IServiceCollection RegisterValidators(this IServiceCollection services)
    //{
    //    // Application katmanındaki validator'ları bulmak için
    //    services.AddValidatorsFromAssembly(typeof(CreateCategoryCommandValidator).Assembly);
    //    return services;
    //}

    private static IServiceCollection RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        return services;
    }
}

// Mock Repository Implementations - Sadece Category CRUD için gerekli minimum
public class MockTaskItemRepository : ITaskItemRepository
{
    public Task<TaskItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<TaskItem?>(null);
    public Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
    public Task AddAsync(TaskItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(TaskItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(TaskItem entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<TaskItem>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
    public Task<IReadOnlyList<TaskItem>> GetTasksByUserIdAsync(int userId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
    public Task<IReadOnlyList<TaskItem>> GetFilteredAsync(int userId, bool? isCompleted = null, Priority? priority = null, DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
    public Task<IReadOnlyList<TaskItem>> GetOverdueTasksAsync(int userId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
    public Task<IReadOnlyList<TaskItem>> GetUpcomingTasksAsync(int userId, int days = 7, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItem>>(new List<TaskItem>());
}

public class MockUserRepository : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<User?>(null);
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(new List<User>());
    public Task AddAsync(User entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(User entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(User entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<User>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<User>>(new List<User>());
    public Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null, CancellationToken ct = default) => Task.FromResult(true);
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult<User?>(null);
    public Task<bool> ValidateCredentialsAsync(string email, string password, CancellationToken ct = default) => Task.FromResult(false);
    public Task<int?> GetUserIdByEmailAsync(string email, CancellationToken ct = default) => Task.FromResult<int?>(null);
}

public class MockTaskItemCategoryRepository : ITaskItemCategoryRepository
{
    public Task<TaskItemCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => Task.FromResult<TaskItemCategory?>(null);
    public Task<IReadOnlyList<TaskItemCategory>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaskItemCategory>>(new List<TaskItemCategory>());
    public Task AddAsync(TaskItemCategory entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task UpdateAsync(TaskItemCategory entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task DeleteAsync(TaskItemCategory entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task<IReadOnlyList<TaskItemCategory>> GetAllIncludeDeletedAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaskItemCategory>>(new List<TaskItemCategory>());
    public Task<IReadOnlyList<TaskItemCategory>> GetByTaskItemIdAsync(int taskItemId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItemCategory>>(new List<TaskItemCategory>());
    public Task<IReadOnlyList<TaskItemCategory>> GetByCategoryIdAsync(int categoryId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItemCategory>>(new List<TaskItemCategory>());
    public Task<TaskItemCategory?> GetByTaskAndCategoryAsync(int taskItemId, int categoryId, CancellationToken ct = default) => Task.FromResult<TaskItemCategory?>(null);
    public Task<IReadOnlyList<TaskItemCategory>> GetByTaskItemIdsAsync(IEnumerable<int> taskItemIds, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<TaskItemCategory>>(new List<TaskItemCategory>());
    public Task DeleteAllByTaskItemIdAsync(int taskItemId, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAllByCategoryIdAsync(int categoryId, CancellationToken ct = default) => Task.CompletedTask;
}
