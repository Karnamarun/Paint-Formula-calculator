namespace PaintTintCalculator.Application.DTOs.DispenseJobs;

public sealed record CreateDispenseJobRequest(
    int ShadeId,
    int BaseId,
    decimal CanSizeLitres);

public sealed record DispenseJobItemDto(
    int Id,
    int ColorantId,
    string ColorantCode,
    string ColorantName,
    decimal DispensedMl,
    decimal Cost);

public sealed record DispenseJobDto(
    int Id,
    int ShadeId,
    string ShadeCode,
    string ShadeName,
    int BaseId,
    string BaseName,
    decimal CanSizeLitres,
    decimal TotalColorantMl,
    decimal TintPercent,
    decimal TotalPrice,
    DateTimeOffset CreatedAt,
    IReadOnlyList<DispenseJobItemDto> Items);
