namespace PaintTintCalculator.Domain.Exceptions;

public sealed class FormulaNotFoundException : DomainException
{
    public int ShadeId { get; }
    public int BaseId { get; }

    public FormulaNotFoundException(int shadeId, int baseId)
        : base($"No formula found for shade ID {shadeId} and base ID {baseId}.", "FORMULA_NOT_FOUND")
    {
        ShadeId = shadeId;
        BaseId = baseId;
    }

    public FormulaNotFoundException(string shadeNameOrCode, string baseName)
        : base($"No formula found for shade '{shadeNameOrCode}' with base '{baseName}'.", "FORMULA_NOT_FOUND")
    {
    }
}
