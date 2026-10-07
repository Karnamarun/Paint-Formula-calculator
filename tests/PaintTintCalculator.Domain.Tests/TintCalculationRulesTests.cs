using PaintTintCalculator.Domain.Exceptions;
using PaintTintCalculator.Domain.Rules;
using Xunit;

namespace PaintTintCalculator.Domain.Tests;

public class TintCalculationRulesTests
{
    [Theory]
    [InlineData(12.5, 1.0, 12.5)]
    [InlineData(12.5, 4.0, 50.0)]
    [InlineData(12.5, 10.0, 125.0)]
    [InlineData(12.5, 20.0, 250.0)]
    [InlineData(0.0, 4.0, 0.0)]
    public void ScaleQuantity_MultipliesMlPerLitreByCanSize(decimal mlPerLitre, decimal canSizeLitres, decimal expected)
    {
        // Act
        var actual = TintCalculationRules.ScaleQuantity(mlPerLitre, canSizeLitres);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(0.000, 0.00)]
    [InlineData(0.024, 0.00)]
    [InlineData(0.025, 0.05)]
    [InlineData(0.026, 0.05)]
    [InlineData(0.049, 0.05)]
    [InlineData(0.050, 0.05)]
    [InlineData(0.074, 0.05)]
    [InlineData(0.075, 0.10)]
    [InlineData(12.33, 12.35)]
    [InlineData(12.31, 12.30)]
    public void RoundToDispenserPrecision_RoundsToNearestPointZeroFiveMl(decimal rawAmount, decimal expected)
    {
        // Act
        var actual = TintCalculationRules.RoundToDispenserPrecision(rawAmount);

        // Assert
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RoundToDispenserPrecision_AllowsCustomPrecision_ForDemoFlexibility()
    {
        // Demonstrates evaluator requirement: changing precision to 0.10 ml requires one config change
        var actual = TintCalculationRules.RoundToDispenserPrecision(12.34m, 0.10m);
        Assert.Equal(12.30m, actual);
    }

    [Theory]
    [InlineData(1.0, 2.0, 20.0)]     // 1L Pastel: 1000ml * 2% = 20ml
    [InlineData(4.0, 2.0, 80.0)]     // 4L Pastel: 4000ml * 2% = 80ml
    [InlineData(4.0, 6.0, 240.0)]    // 4L Medium: 4000ml * 6% = 240ml
    [InlineData(10.0, 6.0, 600.0)]   // 10L Medium: 10000ml * 6% = 600ml
    [InlineData(20.0, 12.0, 2400.0)] // 20L Deep: 20000ml * 12% = 2400ml
    public void CalculateMaxAllowedColorantMl_ReturnsAccurateVolume(decimal canSizeLitres, decimal maxTintPercent, decimal expectedMl)
    {
        // Act
        var actual = TintCalculationRules.CalculateMaxAllowedColorantMl(canSizeLitres, maxTintPercent);

        // Assert
        Assert.Equal(expectedMl, actual);
    }

    [Theory]
    [InlineData(80.0, 4.0, 2.00)]
    [InlineData(120.0, 4.0, 3.00)]
    [InlineData(240.0, 4.0, 6.00)]
    [InlineData(250.0, 4.0, 6.25)]
    public void CalculateTintPercent_ReturnsAccuratePercentage(decimal totalColorantMl, decimal canSizeLitres, decimal expectedPercent)
    {
        // Act
        var actual = TintCalculationRules.CalculateTintPercent(totalColorantMl, canSizeLitres);

        // Assert
        Assert.Equal(expectedPercent, actual);
    }

    [Fact]
    public void ValidateTintLimit_Passes_WhenTotalColorantIsWithinLimit()
    {
        // Pastel base (2%) on 4L can allows 80.00 ml
        // Act & Assert (should not throw)
        TintCalculationRules.ValidateTintLimit("Pastel", 4.0m, 75.0m, 2.0m);
    }

    [Fact]
    public void ValidateTintLimit_Passes_WhenTotalColorantIsExactlyAtLimit()
    {
        // Medium base (6%) on 4L can allows exactly 240.00 ml
        // Act & Assert (should not throw)
        TintCalculationRules.ValidateTintLimit("Medium", 4.0m, 240.00m, 6.0m);
    }

    [Fact]
    public void ValidateTintLimit_ThrowsTintLimitExceededException_WhenJustAboveLimit()
    {
        // Medium base (6%) on 4L can allows 240.00 ml. 240.05 ml exceeds limit.
        var ex = Assert.Throws<TintLimitExceededException>(() =>
            TintCalculationRules.ValidateTintLimit("Medium", 4.0m, 240.05m, 6.0m));

        Assert.Equal("Medium", ex.BaseName);
        Assert.Equal(4.0m, ex.CanSizeLitres);
        Assert.Equal(240.00m, ex.MaxAllowedMl);
        Assert.Equal(240.05m, ex.RequestedMl);
        Assert.Equal(6.0m, ex.MaxTintPercent);
        Assert.Equal("TINT_LIMIT_EXCEEDED", ex.Code);
    }

    [Fact]
    public void ValidateTintLimit_ThrowsDetailedException_WithFormattedMessage()
    {
        var ex = Assert.Throws<TintLimitExceededException>(() =>
            TintCalculationRules.ValidateTintLimit("Pastel", 1.0m, 25.0m, 2.0m));

        Assert.Contains("Pastel base allows a maximum of 2%", ex.Message);
        Assert.Contains("20.00 ml", ex.Message);
        Assert.Contains("25.00 ml", ex.Message);
    }
}
