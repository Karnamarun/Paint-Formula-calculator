using PaintTintCalculator.Domain.Exceptions;

namespace PaintTintCalculator.Domain.ValueObjects;

public sealed record CanSize
{
    public static readonly IReadOnlyList<decimal> SupportedLitres = new[] { 1.0m, 4.0m, 10.0m, 20.0m };

    public decimal Litres { get; }

    public decimal VolumeInMl => Litres * 1000m;

    private CanSize(decimal litres)
    {
        Litres = litres;
    }

    public static CanSize Create(decimal litres)
    {
        if (!IsSupported(litres))
        {
            var supported = string.Join(", ", SupportedLitres.Select(s => $"{s:G29}L"));
            throw new InvalidCanSizeException(litres, supported);
        }

        return new CanSize(litres);
    }

    public static bool IsSupported(decimal litres)
    {
        return SupportedLitres.Contains(litres);
    }

    public override string ToString() => $"{Litres}L";

    public static implicit operator decimal(CanSize canSize) => canSize.Litres;
}
