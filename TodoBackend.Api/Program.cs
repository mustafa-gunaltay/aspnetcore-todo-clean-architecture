using Serilog;
using TodoBackend.Api.Configs;

// Serilog konfigürasyonu
AppUseExtensions.ConfigureSerilog();

try
{
    Log.Information("Starting TodoBackend API");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog'u Host'a ekle ve Services'i register et
    builder.AddSerilog();
    builder.Services.Register(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    // Middleware pipeline'?n? yap?land?r
    app.AppUse(builder.Configuration);

    Log.Information("TodoBackend API started successfully");
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
