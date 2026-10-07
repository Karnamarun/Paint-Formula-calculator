using PaintTintCalculator.Domain.Rules;
using Xunit;

namespace PaintTintCalculator.Domain.Tests;

public class PricingRulesTests
{
    [Theory]
    [InlineData(250.0, 1.0, 250.0)]
    [InlineData(250.0, 4.0, 1000.0)]
    [InlineData(270.0, 4.0, 1080.0)]
    [InlineData(290.0, 10.0, 2900.0)]
    [InlineData(290.0, 20.0, 5800.0)]
    public void CalculateBaseCost_MultipliesPricePerLitreByCanSize(decimal pricePerLitre, decimal canSize, decimal expected)
    {
        var actual = PricingRules.CalculateBaseCost(pricePerLitre, canSize);
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(12.50, 0.80, 10.00)]
    [InlineData(25.40, 0.90, 22.86)]
    [InlineData(10.05, 1.20, 12.06)]
    [InlineData(0.00, 1.20, 0.00)]
    public void CalculateColorantCost_MultipliesDispensedMlByCostPerMl(decimal dispensedMl, decimal costPerMl, decimal expected)
    {
        var actual = PricingRules.CalculateColorantCost(dispensedMl, costPerMl);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CalculateTotalPrice_SumsBaseAndColorants_AndRoundsToTwoDecimals()
    {
        decimal baseCost = 1080.00m;
        decimal colorant1 = PricingRules.CalculateColorantCost(15.25m, 0.80m); // 12.20
        decimal colorant2 = PricingRules.CalculateColorantCost(18.50m, 1.20m); // 22.20
        decimal totalColorant = colorant1 + colorant2; // 34.40

        var actual = PricingRules.CalculateTotalPrice(baseCost, totalColorant);

        Assert.Equal(1114.40m, actual);
    }

    [Fact]
    public void CalculateTotalPrice_HandlesSubCentRoundingCorrectly()
    {
        // 1000.00 + 10.005 -> 1010.01 with AwayFromZero rounding
        var actual = PricingRules.CalculateTotalPrice(1000.00m, 10.005m);
        Assert.Equal(1010.01m, actual);
    }
}
