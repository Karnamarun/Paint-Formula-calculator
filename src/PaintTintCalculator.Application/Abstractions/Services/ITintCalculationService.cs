using PaintTintCalculator.Application.DTOs.Tint;
using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Application.Abstractions.Services;

public interface ITintCalculationService
{
    TintCalculationResultDto Calculate(Shade shade, Base baseEntity, decimal canSizeLitres);
}

