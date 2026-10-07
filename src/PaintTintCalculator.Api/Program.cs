using Microsoft.EntityFrameworkCore;
using PaintTintCalculator.Api.Middleware;
using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Application.Abstractions.Services;
using PaintTintCalculator.Application.Features.DispenseJobs.Commands;
using PaintTintCalculator.Application.Features.Shades.Queries;
using PaintTintCalculator.Application.Features.TintCalculation.Commands;
using PaintTintCalculator.Application.Services;
using PaintTintCalculator.Infrastructure.Persistence;
using PaintTintCalculator.Infrastructure.Persistence.Repositories;
using PaintTintCalculator.Infrastructure.Seed;

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

// Persistence Registration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? "Data Source=painttint.db";

builder.Services.AddDbContext<PaintTintDbContext>(options =>
{
    options.UseSqlite(connectionString);
});

builder.Services.AddScoped<IShadeRepository, ShadeRepository>();
builder.Services.AddScoped<IBaseRepository, BaseRepository>();
builder.Services.AddScoped<IDispenseJobRepository, DispenseJobRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<SeedDataService>();

// Application Services Registration
builder.Services.AddScoped<ITintCalculationService, TintCalculationService>();

// Feature / Use Case Handlers Registration
builder.Services.AddScoped<SearchShadesQueryHandler>();
builder.Services.AddScoped<GetShadeDetailsQueryHandler>();
builder.Services.AddScoped<GetBasesQueryHandler>();
builder.Services.AddScoped<CalculateTintCommandHandler>();
builder.Services.AddScoped<CreateDispenseJobCommandHandler>();
builder.Services.AddScoped<GetRecentDispenseJobsQueryHandler>();

var app = builder.Build();

// Seed initial database data
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedDataService>();
    await seeder.InitializeAsync();
}

// Global Exception Handling Middleware
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
