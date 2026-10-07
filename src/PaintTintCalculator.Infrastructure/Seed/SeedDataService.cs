using Microsoft.EntityFrameworkCore;
using PaintTintCalculator.Domain.Entities;
using PaintTintCalculator.Infrastructure.Persistence;

namespace PaintTintCalculator.Infrastructure.Seed;

public class SeedDataService
{
    private readonly PaintTintDbContext _context;

    public SeedDataService(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);

        if (!await _context.Bases.AnyAsync(cancellationToken))
        {
            await SeedBasesAsync(cancellationToken);
            await SaveSeededEntitiesAsync("Bases", cancellationToken);
        }

        if (!await _context.Colorants.AnyAsync(cancellationToken))
        {
            await SeedColorantsAsync(cancellationToken);
            await SaveSeededEntitiesAsync("Colorants", cancellationToken);
        }

        if (!await _context.Shades.AnyAsync(cancellationToken))
        {
            await SeedShadesAndFormulasAsync(cancellationToken);
            await SaveSeededEntitiesAsync("Shades", cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveSeededEntitiesAsync(string tableName, CancellationToken cancellationToken)
    {
        if (!_context.Database.IsSqlServer())
        {
            await _context.SaveChangesAsync(cancellationToken);
            return;
        }

        await _context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await _context.Database.ExecuteSqlRawAsync(GetIdentityInsertStatement(tableName, enabled: true), cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await _context.Database.ExecuteSqlRawAsync(GetIdentityInsertStatement(tableName, enabled: false), cancellationToken);
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    private static string GetIdentityInsertStatement(string tableName, bool enabled)
    {
        return (tableName, enabled) switch
        {
            ("Bases", true) => "SET IDENTITY_INSERT dbo.[Bases] ON",
            ("Bases", false) => "SET IDENTITY_INSERT dbo.[Bases] OFF",
            ("Colorants", true) => "SET IDENTITY_INSERT dbo.[Colorants] ON",
            ("Colorants", false) => "SET IDENTITY_INSERT dbo.[Colorants] OFF",
            ("Shades", true) => "SET IDENTITY_INSERT dbo.[Shades] ON",
            ("Shades", false) => "SET IDENTITY_INSERT dbo.[Shades] OFF",
            _ => throw new ArgumentOutOfRangeException(nameof(tableName), tableName, "Unsupported seed table.")
        };
    }

    private async Task SeedBasesAsync(CancellationToken cancellationToken)
    {
        var bases = new List<Base>
        {
            new() { Id = 1, Name = "Pastel", MaxTintPercent = 2.00m, PricePerLitre = 250.00m },
            new() { Id = 2, Name = "Medium", MaxTintPercent = 6.00m, PricePerLitre = 270.00m },
            new() { Id = 3, Name = "Deep", MaxTintPercent = 12.00m, PricePerLitre = 290.00m }
        };

        await _context.Bases.AddRangeAsync(bases, cancellationToken);
    }

    private async Task SeedColorantsAsync(CancellationToken cancellationToken)
    {
        var colorants = new List<Colorant>
        {
            new() { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.8000m },
            new() { Id = 2, Code = "C02", Name = "Oxide Red", CostPerMl = 0.9000m },
            new() { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.2000m },
            new() { Id = 4, Code = "C04", Name = "Yellow Oxide", CostPerMl = 0.8500m }
        };

        await _context.Colorants.AddRangeAsync(colorants, cancellationToken);
    }

    private async Task SeedShadesAndFormulasAsync(CancellationToken cancellationToken)
    {
        // OM-201 Ocean Mist (#7FA7B5)
        var oceanMist = new Shade
        {
            Id = 1,
            Code = "OM-201",
            Name = "Ocean Mist",
            HexColor = "#7FA7B5"
        };

        // Ocean Mist - Pastel Base (Pastel max 2% = 20ml/L. Here total = 7.5ml/L -> 0.75%)
        oceanMist.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 3, MlPerLitre = 5.0000m }); // Phthalo Blue
        oceanMist.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 1, MlPerLitre = 2.5000m }); // Black

        // Ocean Mist - Medium Base (Medium max 6% = 60ml/L. Here total = 50.0ml/L -> 5.00%)
        oceanMist.FormulaItems.Add(new FormulaItem { BaseId = 2, ColorantId = 3, MlPerLitre = 35.0000m }); // Phthalo Blue
        oceanMist.FormulaItems.Add(new FormulaItem { BaseId = 2, ColorantId = 1, MlPerLitre = 15.0000m }); // Black

        // TR-115 Terracotta (#C4663F)
        var terracotta = new Shade
        {
            Id = 2,
            Code = "TR-115",
            Name = "Terracotta",
            HexColor = "#C4663F"
        };

        // Terracotta - Medium Base (Medium max 6% = 60ml/L. Here total = 50.0ml/L -> 5.00%)
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 2, ColorantId = 2, MlPerLitre = 28.0000m }); // Oxide Red
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 2, ColorantId = 4, MlPerLitre = 18.0000m }); // Yellow Oxide
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 2, ColorantId = 1, MlPerLitre = 4.0000m });  // Black

        // Terracotta - Deep Base (Deep max 12% = 120ml/L. Here total = 103.0ml/L -> 10.30%)
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 3, ColorantId = 2, MlPerLitre = 60.0000m }); // Oxide Red
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 3, ColorantId = 4, MlPerLitre = 35.0000m }); // Yellow Oxide
        terracotta.FormulaItems.Add(new FormulaItem { BaseId = 3, ColorantId = 1, MlPerLitre = 8.0000m });  // Black

        // SG-305 Sage Green (#8A9A86)
        var sageGreen = new Shade
        {
            Id = 3,
            Code = "SG-305",
            Name = "Sage Green",
            HexColor = "#8A9A86"
        };
        // Sage Green - Pastel Base (total 15 ml/L -> 1.50% <= 2%)
        sageGreen.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 4, MlPerLitre = 9.0000m });  // Yellow Oxide
        sageGreen.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 3, MlPerLitre = 4.0000m });  // Phthalo Blue
        sageGreen.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 1, MlPerLitre = 2.0000m });  // Black

        // SR-402 Sunset Rose (#C87D7D)
        var sunsetRose = new Shade
        {
            Id = 4,
            Code = "SR-402",
            Name = "Sunset Rose",
            HexColor = "#C87D7D"
        };
        // Sunset Rose - Pastel Base (total 16 ml/L -> 1.60% <= 2%)
        sunsetRose.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 2, MlPerLitre = 10.0000m }); // Oxide Red
        sunsetRose.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 4, MlPerLitre = 4.0000m });  // Yellow Oxide
        sunsetRose.FormulaItems.Add(new FormulaItem { BaseId = 1, ColorantId = 1, MlPerLitre = 2.0000m });  // Black

        await _context.Shades.AddRangeAsync(new[] { oceanMist, terracotta, sageGreen, sunsetRose }, cancellationToken);
    }
}
