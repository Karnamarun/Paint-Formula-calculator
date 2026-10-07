namespace PaintTintCalculator.Wpf.Models;

public class ShadeModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;

    public string DisplayText => $"{Code} - {Name}";
}

public class BaseModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MaxTintPercent { get; set; }
    public decimal PricePerLitre { get; set; }

    public string DisplayText => $"{Name} (Max {MaxTintPercent:G29}%)";
}

public class CanSizeOption
{
    public decimal Litres { get; }
    public string DisplayText { get; }

    public CanSizeOption(decimal litres)
    {
        Litres = litres;
        DisplayText = $"{litres:G29} Litre{(litres > 1 ? "s" : "")}";
    }

    public static readonly IReadOnlyList<CanSizeOption> DefaultOptions = new[]
    {
        new CanSizeOption(1.0m),
        new CanSizeOption(4.0m),
        new CanSizeOption(10.0m),
        new CanSizeOption(20.0m)
    };
}

public class FormulaRowModel
{
    public int ColorantId { get; set; }
    public string ColorantCode { get; set; } = string.Empty;
    public string ColorantName { get; set; } = string.Empty;
    public decimal MlPerLitre { get; set; }
    public decimal DispensedMl { get; set; }
    public decimal CostPerMl { get; set; }
    public decimal TotalCost { get; set; }
}

public class ApiErrorResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
}

