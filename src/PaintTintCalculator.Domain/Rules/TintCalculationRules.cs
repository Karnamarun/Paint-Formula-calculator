using PaintTintCalculator.Domain.Exceptions;

namespace PaintTintCalculator.Domain.Rules;

public static class TintCalculationRules
{
    /// <summary>
    /// Dispenser precision increment in milliliters (default 0.05 ml).
    /// Can be easily modified during evaluation.
    /// </summary>
    public const decimal DispenserPrecisionMl = 0.05m;

    public static decimal ScaleQuantity(decimal mlPerLitre, decimal canSizeLitres)
    {
        return mlPerLitre * canSizeLitres;
    }

    public static decimal RoundToDispenserPrecision(decimal amount, decimal precision = DispenserPrecisionMl)
    {
        if (precision <= 0m)
        {
            return amount;
        }

        return Math.Round(amount / precision, 0, MidpointRounding.AwayFromZero) * precision;
    }

    public static decimal CalculateDispensedMl(decimal mlPerLitre, decimal canSizeLitres, decimal precision = DispenserPrecisionMl)
    {
        var scaled = ScaleQuantity(mlPerLitre, canSizeLitres);
        return RoundToDispenserPrecision(scaled, precision);
    }

    public static decimal CalculateMaxAllowedColorantMl(decimal canSizeLitres, decimal maxTintPercent)
    {
        var canVolumeMl = canSizeLitres * 1000m;
        return canVolumeMl * maxTintPercent / 100m;
    }

    public static decimal CalculateTintPercent(decimal totalColorantMl, decimal canSizeLitres)
    {
        var canVolumeMl = canSizeLitres * 1000m;
        if (canVolumeMl == 0m)
        {
            return 0m;
        }

        return Math.Round((totalColorantMl / canVolumeMl) * 100m, 2, MidpointRounding.AwayFromZero);
    }

    public static void ValidateTintLimit(
        string baseName,
        decimal canSizeLitres,
        decimal totalColorantMl,
        decimal maxTintPercent)
    {
        var maxAllowed = CalculateMaxAllowedColorantMl(canSizeLitres, maxTintPercent);
        var calculatedTintPercent = CalculateTintPercent(totalColorantMl, canSizeLitres);

        if (totalColorantMl > maxAllowed)
        {
            throw new TintLimitExceededException(
                baseName,
                canSizeLitres,
                maxAllowed,
                totalColorantMl,
                maxTintPercent,
                calculatedTintPercent);
        }
    }
}
