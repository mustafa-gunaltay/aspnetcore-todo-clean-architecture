using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TodoBackend.Application.Features.TodoCategory.Commands.CreateCategory;
using TodoBackend.Domain.Enums;
using TodoBackend.Domain.Interfaces;
using TodoBackend.Domain.Models;
using TodoBackend.Infrastructure;
using TodoBackend.Infrastructure.BuildingBlocks.Implementations;
using TodoBackend.Infrastructure.Repositories;
using TodoBackend.Application.Features.BuildingBlocks.Behaviors;
using Microsoft.OpenApi.Models;

namespace TodoBackend.Api.Configs;

public static class DependencyInjection
{
    public static IServiceCollection Register(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterDbContext(configuration)
            .RegisterRepositories()
            .RegisterMediatR()
            .RegisterValidators()
            .RegisterCurrentUser()
            .RegisterSwagger();

        return services;
    }

    private static IServiceCollection RegisterDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TodoBackendDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("TodoBackendDbConnection")), // appsettings.json'daki mssql db connection stringini verir
            ServiceLifetime.Scoped);
        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {

        // repository kayıtları
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITaskItemCategoryRepository, TaskItemCategoryRepository>();

        services.AddScoped<ITodoBackendUnitOfWork, TodoBackendUnitOfWork>();

        return services;
    }

    private static IServiceCollection RegisterMediatR(this IServiceCollection services)
    {
        // Application katmanındaki handler'ları bulmak için
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCategoryCommandHandler>();
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        });
        return services;
    }

    private static IServiceCollection RegisterValidators(this IServiceCollection services)
    {
        // NOT: Her yeni validator sinifi yazdigimizda ayri ayri buraya eklememize gerek yok
        // Tek Kural: Validator'ınız Application katmanında olsun ve AbstractValidator<T> türünden türesin.

        // FluentValidation.DependencyInjectionExtensions paketi extension method'unu Microsoft.Extensions.DependencyInjection namespace'ine ekler
        services.AddValidatorsFromAssembly(typeof(CreateCategoryCommandValidator).Assembly);
        
        return services;
    }

    private static IServiceCollection RegisterCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        return services;
    }

    private static IServiceCollection RegisterSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TodoBackend API",
                Version = "v1",
                Description = "A Todo application API built with Clean Architecture",
                Contact = new OpenApiContact
                {
                    Name = "TodoBackend Team",
                    Email = "info@todobackend.com"
                }
            });

            // Enable annotations for Swagger
            c.EnableAnnotations();
        });
        
        return services;
    }
}

