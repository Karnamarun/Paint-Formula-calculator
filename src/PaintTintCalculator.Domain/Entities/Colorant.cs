namespace PaintTintCalculator.Domain.Entities;

public class Colorant
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CostPerMl { get; set; }

    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();
    public ICollection<DispenseJobItem> DispenseJobItems { get; set; } = new List<DispenseJobItem>();
}
