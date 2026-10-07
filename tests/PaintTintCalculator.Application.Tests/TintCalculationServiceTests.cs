using PaintTintCalculator.Application.Services;
using PaintTintCalculator.Domain.Entities;
using PaintTintCalculator.Domain.Exceptions;
using Xunit;

namespace PaintTintCalculator.Application.Tests;

public class TintCalculationServiceTests
{
    private readonly TintCalculationService _service = new();

    private static (Shade shade, Base pastelBase, Base mediumBase, Base deepBase) CreateSampleData()
    {
        var pastelBase = new Base { Id = 1, Name = "Pastel", MaxTintPercent = 2.0m, PricePerLitre = 250.0m };
        var mediumBase = new Base { Id = 2, Name = "Medium", MaxTintPercent = 6.0m, PricePerLitre = 270.0m };
        var deepBase = new Base { Id = 3, Name = "Deep", MaxTintPercent = 12.0m, PricePerLitre = 290.0m };

        var black = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };
        var blue = new Colorant { Id = 3, Code = "C03", Name = "Phthalo Blue", CostPerMl = 1.20m };

        // Ocean Mist shade
        var shade = new Shade
        {
            Id = 1,
            Code = "OM-201",
            Name = "Ocean Mist",
            HexColor = "#7FA7B5"
        };

        // Pastel base formula for Ocean Mist: 5.0 ml/L Blue, 2.5 ml/L Black
        // On 4L can: 5*4 = 20ml Blue, 2.5*4 = 10ml Black -> Total 30ml (0.75% < 2% max)
        shade.FormulaItems.Add(new FormulaItem
        {
            Id = 1,
            ShadeId = 1,
            BaseId = pastelBase.Id,
            ColorantId = blue.Id,
            MlPerLitre = 5.0m,
            Colorant = blue,
            Base = pastelBase
        });
        shade.FormulaItems.Add(new FormulaItem
        {
            Id = 2,
            ShadeId = 1,
            BaseId = pastelBase.Id,
            ColorantId = black.Id,
            MlPerLitre = 2.5m,
            Colorant = black,
            Base = pastelBase
        });

        // Medium base formula for Ocean Mist: 35.0 ml/L Blue, 15.0 ml/L Black
        // On 4L can: 35*4 = 140ml Blue, 15*4 = 60ml Black -> Total 200ml (5% <= 6% max)
        shade.FormulaItems.Add(new FormulaItem
        {
            Id = 3,
            ShadeId = 1,
            BaseId = mediumBase.Id,
            ColorantId = blue.Id,
            MlPerLitre = 35.0m,
            Colorant = blue,
            Base = mediumBase
        });
        shade.FormulaItems.Add(new FormulaItem
        {
            Id = 4,
            ShadeId = 1,
            BaseId = mediumBase.Id,
            ColorantId = black.Id,
            MlPerLitre = 15.0m,
            Colorant = black,
            Base = mediumBase
        });

        return (shade, pastelBase, mediumBase, deepBase);
    }

    [Fact]
    public void Calculate_ProducesAccurateScaledQuantitiesAndPricing()
    {
        // Arrange
        var (shade, pastelBase, _, _) = CreateSampleData();

        // Act (4L can)
        var result = _service.Calculate(shade, pastelBase, 4.0m);

        // Assert
        Assert.Equal("OM-201", result.ShadeCode);
        Assert.Equal("Pastel", result.BaseName);
        Assert.Equal(4.0m, result.CanSizeLitres);
        Assert.Equal(2, result.Items.Count);

        // Blue: 5.0 * 4 = 20.00 ml @ 1.20 = 24.00
        var blueItem = result.Items.First(i => i.ColorantCode == "C03");
        Assert.Equal(20.00m, blueItem.DispensedMl);
        Assert.Equal(24.00m, blueItem.TotalCost);

        // Black: 2.5 * 4 = 10.00 ml @ 0.80 = 8.00
        var blackItem = result.Items.First(i => i.ColorantCode == "C01");
        Assert.Equal(10.00m, blackItem.DispensedMl);
        Assert.Equal(8.00m, blackItem.TotalCost);

        // Totals
        Assert.Equal(30.00m, result.TotalColorantMl);
        Assert.Equal(0.75m, result.TintPercent); // 30 / 4000 = 0.75%
        Assert.Equal(1000.00m, result.BaseCost); // 250 * 4 = 1000
        Assert.Equal(32.00m, result.ColorantCost); // 24 + 8 = 32
        Assert.Equal(1032.00m, result.TotalPrice); // 1000 + 32 = 1032.00
    }

    [Fact]
    public void Calculate_ThrowsFormulaNotFoundException_WhenNoFormulaExistsForBase()
    {
        // Arrange (Deep base has no formula configured for this shade)
        var (shade, _, _, deepBase) = CreateSampleData();

        // Act & Assert
        var ex = Assert.Throws<FormulaNotFoundException>(() =>
            _service.Calculate(shade, deepBase, 4.0m));

        Assert.Equal("FORMULA_NOT_FOUND", ex.Code);
    }

    [Fact]
    public void Calculate_ThrowsInvalidCanSizeException_WhenCanSizeIsNotSupported()
    {
        // Arrange
        var (shade, pastelBase, _, _) = CreateSampleData();

        // Act & Assert (2.5L is unsupported)
        Assert.Throws<InvalidCanSizeException>(() =>
            _service.Calculate(shade, pastelBase, 2.5m));
    }

    [Fact]
    public void Calculate_ThrowsTintLimitExceededException_WhenFormulaExceedsMaxTintLimit()
    {
        // Arrange
        var baseEntity = new Base { Id = 1, Name = "Pastel", MaxTintPercent = 2.0m, PricePerLitre = 250.0m };
        var colorant = new Colorant { Id = 1, Code = "C01", Name = "Black", CostPerMl = 0.80m };
        var shade = new Shade { Id = 2, Code = "DARK-01", Name = "Dark Shadow", HexColor = "#111111" };

        // 25.0 ml/L on 1L can requires 25 ml (2.5% > 2% max allowed)
        shade.FormulaItems.Add(new FormulaItem
        {
            Id = 10,
            ShadeId = 2,
            BaseId = baseEntity.Id,
            ColorantId = colorant.Id,
            MlPerLitre = 25.0m,
            Colorant = colorant,
            Base = baseEntity
        });

        // Act & Assert
        var ex = Assert.Throws<TintLimitExceededException>(() =>
            _service.Calculate(shade, baseEntity, 1.0m));

        Assert.Equal("TINT_LIMIT_EXCEEDED", ex.Code);
        Assert.Equal(20.00m, ex.MaxAllowedMl); // 1000 * 2% = 20ml
        Assert.Equal(25.00m, ex.RequestedMl);
    }
}
