using PaintTintCalculator.Api.Extensions;
using PaintTintCalculator.Api.Middleware;
using PaintTintCalculator.Infrastructure.Seed;

foreach (var startPath in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
{
    DirectoryInfo? directory = new(startPath);
    while (directory != null)
    {
        var envFile = Path.Combine(directory.FullName, ".env");
        if (File.Exists(envFile))
        {
            DotNetEnv.Env.Load(envFile);
            break;
        }

        directory = directory.Parent;
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// CORS configuration for desktop and web clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Register Infrastructure persistence (DbContext, Repositories, UoW)
builder.Services.AddInfrastructure(builder.Configuration);

// Register Application use cases and services
builder.Services.AddApplicationServices();

var app = builder.Build();

// Seed initial database data on startup
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seeder.InitializeAsync();
}

// Global Exception Handling Middleware (translates domain exceptions to structured HTTP errors)
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible for WebApplicationFactory in integration tests
public partial class Program { }
