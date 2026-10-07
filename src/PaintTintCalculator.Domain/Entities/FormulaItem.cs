namespace PaintTintCalculator.Domain.Entities;

public class FormulaItem
{
    public int Id { get; set; }
    public int ShadeId { get; set; }
    public int BaseId { get; set; }
    public int ColorantId { get; set; }
    public decimal MlPerLitre { get; set; }

    public Shade Shade { get; set; } = null!;
    public Base Base { get; set; } = null!;
    public Colorant Colorant { get; set; } = null!;
}
