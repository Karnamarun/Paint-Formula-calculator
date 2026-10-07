using PaintTintCalculator.Wpf.Models;

namespace PaintTintCalculator.Wpf.Services;

public interface IApiClient
{
    string BaseAddress { get; }
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ShadeModel>> GetShadesAsync(string? search = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BaseModel>> GetBasesAsync(CancellationToken cancellationToken = default);
    Task<CalculationResponseModel> CalculateTintAsync(int shadeId, int baseId, decimal canSizeLitres, CancellationToken cancellationToken = default);
    Task<DispenseResponseModel> CreateDispenseJobAsync(int shadeId, int baseId, decimal canSizeLitres, CancellationToken cancellationToken = default);
}

public class CalculationResponseModel
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int ShadeId { get; set; }
    public string ShadeCode { get; set; } = string.Empty;
    public string ShadeName { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public int BaseId { get; set; }
    public string BaseName { get; set; } = string.Empty;
    public decimal BasePricePerLitre { get; set; }
    public decimal CanSizeLitres { get; set; }
    public List<FormulaRowModel> Items { get; set; } = new();
    public decimal TotalColorantMl { get; set; }
    public decimal TintPercent { get; set; }
    public decimal MaxTintPercent { get; set; }
    public decimal BaseCost { get; set; }
    public decimal ColorantCost { get; set; }
    public decimal TotalPrice { get; set; }
}

public class DispenseResponseModel
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public int JobId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

