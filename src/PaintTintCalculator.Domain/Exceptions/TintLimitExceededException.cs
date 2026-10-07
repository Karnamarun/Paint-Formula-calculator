namespace PaintTintCalculator.Domain.Exceptions;

public sealed class TintLimitExceededException : DomainException
{
    public string BaseName { get; }
    public decimal CanSizeLitres { get; }
    public decimal MaxAllowedMl { get; }
    public decimal RequestedMl { get; }
    public decimal MaxTintPercent { get; }
    public decimal CalculatedTintPercent { get; }

    public TintLimitExceededException(
        string baseName,
        decimal canSizeLitres,
        decimal maxAllowedMl,
        decimal requestedMl,
        decimal maxTintPercent,
        decimal calculatedTintPercent)
        : base(
            $"Tint limit exceeded. {baseName} base allows a maximum of {maxTintPercent:G29}% ({maxAllowedMl:F2} ml for {canSizeLitres:G29}L can), but current formula requires {calculatedTintPercent:F2}% ({requestedMl:F2} ml).",
            "TINT_LIMIT_EXCEEDED")
    {
        BaseName = baseName;
        CanSizeLitres = canSizeLitres;
        MaxAllowedMl = maxAllowedMl;
        RequestedMl = requestedMl;
        MaxTintPercent = maxTintPercent;
        CalculatedTintPercent = calculatedTintPercent;
    }
}
