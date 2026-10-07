namespace PaintTintCalculator.Application.DTOs.Shades;

public sealed record ShadeSummaryDto(
    int Id,
    string Code,
    string Name,
    string HexColor);

public sealed record BaseDto(
    int Id,
    string Name,
    decimal MaxTintPercent,
    decimal PricePerLitre);

public sealed record FormulaItemDto(
    int ColorantId,
    string ColorantCode,
    string ColorantName,
    decimal MlPerLitre,
    decimal CostPerMl);

public sealed record ShadeDetailsDto(
    int Id,
    string Code,
    string Name,
    string HexColor,
    IReadOnlyList<FormulaItemDto> Formulas);
