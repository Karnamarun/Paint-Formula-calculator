namespace PaintTintCalculator.Domain.Entities;

public class Shade
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;

    public ICollection<FormulaItem> FormulaItems { get; set; } = new List<FormulaItem>();
    public ICollection<DispenseJob> DispenseJobs { get; set; } = new List<DispenseJob>();
}

