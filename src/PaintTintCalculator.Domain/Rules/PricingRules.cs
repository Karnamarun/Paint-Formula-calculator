namespace PaintTintCalculator.Domain.Rules;

public static class PricingRules
{
    public static decimal CalculateBaseCost(decimal pricePerLitre, decimal canSizeLitres)
    {
        return pricePerLitre * canSizeLitres;
    }

    public static decimal CalculateColorantCost(decimal dispensedMl, decimal costPerMl)
    {
        return dispensedMl * costPerMl;
    }

    public static decimal CalculateTotalPrice(decimal baseCost, decimal totalColorantCost)
    {
        return Math.Round(baseCost + totalColorantCost, 2, MidpointRounding.AwayFromZero);
    }
}
