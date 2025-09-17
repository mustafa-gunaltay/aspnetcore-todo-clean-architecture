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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoBackend.Api.Services;
using TodoBackend.Infrastructure.Services;
using System.Security.Cryptography;

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
            .RegisterAuthentication(configuration)  // YENİ: Authentication eklendi
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
        services.AddScoped<IJwtService, JwtService>(); // JWT Service eklendi
        services.AddScoped<IPasswordHasher, PasswordHasher>(); // Password Hasher eklendi
        services.AddScoped<IKeyGenerationService, KeyGenerationService>(); // YENİ: Key Generation Service

        return services;
    }

    // GÜNCELLENEN METHOD: JWT Authentication yapılandırması
    private static IServiceCollection RegisterAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        // JWT Token doğrulama ayarları
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,                                           // Token'ı kim verdi kontrol et
                ValidateAudience = true,                                        // Token kime verildi kontrol et
                ValidateLifetime = true,                                        // Token süresi geçmiş mi kontrol et
                ValidateIssuerSigningKey = true,                               // Token imzası doğru mu kontrol et
                ValidIssuer = configuration["Jwt:Issuer"],                     // Geçerli token verici
                ValidAudience = configuration["Jwt:Audience"],                 // Geçerli token alıcısı
                IssuerSigningKey = GetValidationKey(configuration),            // YENİ: Asymmetric/Symmetric key seçimi
                ClockSkew = TimeSpan.FromMinutes(5)                           // Token süre toleransı
            };
        });

        return services;
    }

    /// <summary>
    /// Konfigürasyona göre token doğrulama için gerekli key'i döndürür
    /// </summary>
    private static SecurityKey GetValidationKey(IConfiguration configuration)
    {
        var useAsymmetricKeys = configuration.GetValue<bool>("Jwt:UseAsymmetricKeys");

        if (useAsymmetricKeys)
        {
            // Production: RSA Public Key kullan
            return GetRsaValidationKey(configuration);
        }
        else
        {
            // Development: Symmetric Key kullan
            return GetSymmetricValidationKey(configuration);
        }
    }

    /// <summary>
    /// RSA public key ile token doğrulama key'i oluşturur
    /// </summary>
    private static SecurityKey GetRsaValidationKey(IConfiguration configuration)
    {
        var publicKeyPath = configuration["Jwt:PublicKeyPath"];
        
        if (string.IsNullOrEmpty(publicKeyPath) || !File.Exists(publicKeyPath))
        {
            // Fallback to symmetric key
            return GetSymmetricValidationKey(configuration);
        }

        try
        {
            var publicKeyContent = File.ReadAllText(publicKeyPath);
            var rsa = RSA.Create();
            rsa.ImportFromPem(publicKeyContent);
            return new RsaSecurityKey(rsa);
        }
        catch
        {
            // Fallback to symmetric key
            return GetSymmetricValidationKey(configuration);
        }
    }

    /// <summary>
    /// Symmetric key ile token doğrulama key'i oluşturur
    /// </summary>
    private static SecurityKey GetSymmetricValidationKey(IConfiguration configuration)
    {
        return new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "DefaultKey"));
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
            
            // YENİ: JWT Authentication için Swagger yapılandırması
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }
}

