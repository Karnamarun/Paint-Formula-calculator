namespace PaintTintCalculator.Domain.Entities;

public class DispenseJobItem
{
    public int Id { get; set; }
    public int DispenseJobId { get; set; }
    public int ColorantId { get; set; }
    public decimal DispensedMl { get; set; }
    public decimal Cost { get; set; }

    public DispenseJob DispenseJob { get; set; } = null!;
    public Colorant Colorant { get; set; } = null!;
}

