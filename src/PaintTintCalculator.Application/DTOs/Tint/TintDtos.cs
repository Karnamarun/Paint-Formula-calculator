namespace PaintTintCalculator.Application.DTOs.Tint;

public sealed record CalculateTintRequest(
    int ShadeId,
    int BaseId,
    decimal CanSizeLitres);

public sealed record TintCalculationItemDto(
    int ColorantId,
    string ColorantCode,
    string ColorantName,
    decimal MlPerLitre,
    decimal DispensedMl,
    decimal CostPerMl,
    decimal TotalCost);

public sealed record TintCalculationResultDto(
    int ShadeId,
    string ShadeCode,
    string ShadeName,
    string HexColor,
    int BaseId,
    string BaseName,
    decimal BasePricePerLitre,
    decimal CanSizeLitres,
    IReadOnlyList<TintCalculationItemDto> Items,
    decimal TotalColorantMl,
    decimal TintPercent,
    decimal MaxTintPercent,
    decimal BaseCost,
    decimal ColorantCost,
    decimal TotalPrice);

