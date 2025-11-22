using Serilog;
using TodoBackend.Api.Configs;
using Akka.Actor;
using System.Text.Json.Serialization;

// Serilog konfigürasyonu
AppUseExtensions.ConfigureSerilog();

try
{
    Log.Information("Starting TodoBackend API with Akka.NET Actor System");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u Host'a ekle ve Services'i register et
    builder.AddSerilog();
    builder.Services.Register(builder.Configuration);
    
    // Controllers with JSON options for enum string serialization
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Enum'ları string olarak serialize et (0, 1, 2 yerine "Low", "Medium", "High")
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            
            // Property name'leri camelCase olarak serialize et (opsiyonel)
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        });
    
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    // Middleware pipeline'ını yapılandır
    app.AppUse(builder.Configuration);

    // ActorSystem lifecycle yönetimi
    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    var actorSystem = app.Services.GetRequiredService<ActorSystem>();

    lifetime.ApplicationStopping.Register(() =>
    {
        Log.Information("Gracefully shutting down Akka.NET Actor System");
        actorSystem.Terminate().Wait(TimeSpan.FromSeconds(30));
        Log.Information("Akka.NET Actor System shut down completed");
    });

    Log.Information("TodoBackend API started successfully with Actor System: {ActorSystemName}", 
        actorSystem.Name);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TodoBackend API failed to start");
}
finally
{
    Log.CloseAndFlush();
}
