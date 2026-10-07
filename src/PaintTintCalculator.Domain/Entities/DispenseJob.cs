namespace PaintTintCalculator.Domain.Entities;

public class DispenseJob
{
    public int Id { get; set; }
    public int ShadeId { get; set; }
    public int BaseId { get; set; }
    public decimal CanSizeLitres { get; set; }
    public decimal TotalColorantMl { get; set; }
    public decimal TintPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Shade Shade { get; set; } = null!;
    public Base Base { get; set; } = null!;
    public ICollection<DispenseJobItem> Items { get; set; } = new List<DispenseJobItem>();
}

