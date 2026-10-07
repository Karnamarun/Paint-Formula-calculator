namespace PaintTintCalculator.Domain.Entities;

public class Base
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MaxTintPercent { get; set; }
    public decimal PricePerLitre { get; set; }

    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();
    public ICollection<DispenseJob> DispenseJobs { get; set; } = new List<DispenseJob>();
}

