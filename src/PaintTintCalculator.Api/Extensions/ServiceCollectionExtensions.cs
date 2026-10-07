using Microsoft.EntityFrameworkCore;
using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Application.Abstractions.Services;
using PaintTintCalculator.Application.Features.DispenseJobs.Commands;
using PaintTintCalculator.Application.Features.Shades.Queries;
using PaintTintCalculator.Application.Features.TintCalculation.Commands;
using PaintTintCalculator.Application.Services;
using PaintTintCalculator.Infrastructure.Persistence;
using PaintTintCalculator.Infrastructure.Persistence.Repositories;
using PaintTintCalculator.Infrastructure.Seed;

namespace PaintTintCalculator.Api.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Infrastructure persistence services: DbContext, repositories, and UnitOfWork.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=painttint.db";

        services.AddDbContext<PaintTintDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IShadeRepository, ShadeRepository>();
        services.AddScoped<IBaseRepository, BaseRepository>();
        services.AddScoped<IDispenseJobRepository, DispenseJobRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<SeedDataService>();

        return services;
    }

    /// <summary>
    /// Registers Application services: calculation engine and all use case handlers.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ITintCalculationService, TintCalculationService>();

        services.AddScoped<SearchShadesQueryHandler>();
        services.AddScoped<GetShadeDetailsQueryHandler>();
        services.AddScoped<GetBasesQueryHandler>();
        services.AddScoped<CalculateTintCommandHandler>();
        services.AddScoped<CreateDispenseJobCommandHandler>();
        services.AddScoped<GetRecentDispenseJobsQueryHandler>();

        return services;
    }
}

