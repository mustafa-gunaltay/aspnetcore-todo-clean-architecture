using TodoBackend.Api.Configs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Register(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(); // YEN?: CORS middleware - Authentication'dan önce olmal?

app.UseAuthentication();  // JWT token do?rulamas?
app.UseAuthorization();   // Yetkilendirme kontrolü

app.MapControllers();

app.Run();
