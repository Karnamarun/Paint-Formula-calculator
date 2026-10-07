using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Application.Abstractions.Services;
using PaintTintCalculator.Application.DTOs.Tint;
using PaintTintCalculator.Domain.Exceptions;

namespace PaintTintCalculator.Application.Features.TintCalculation.Commands;

public class CalculateTintCommandHandler
{
    private readonly IShadeRepository _shadeRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly ITintCalculationService _tintCalculationService;

    public CalculateTintCommandHandler(
        IShadeRepository shadeRepository,
        IBaseRepository baseRepository,
        ITintCalculationService tintCalculationService)
    {
        _shadeRepository = shadeRepository;
        _baseRepository = baseRepository;
        _tintCalculationService = tintCalculationService;
    }

    public async Task<TintCalculationResultDto> HandleAsync(CalculateTintRequest request, CancellationToken cancellationToken = default)
    {
        var shade = await _shadeRepository.GetByIdWithFormulaAsync(request.ShadeId, cancellationToken);
        if (shade == null)
        {
            throw new DomainException($"Shade with ID {request.ShadeId} was not found.", "SHADE_NOT_FOUND");
        }

        var baseEntity = await _baseRepository.GetByIdAsync(request.BaseId, cancellationToken);
        if (baseEntity == null)
        {
            throw new DomainException($"Base with ID {request.BaseId} was not found.", "BASE_NOT_FOUND");
        }

        return _tintCalculationService.Calculate(shade, baseEntity, request.CanSizeLitres);
    }
}

