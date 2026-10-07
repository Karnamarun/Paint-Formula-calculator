using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Application.Abstractions.Services;
using PaintTintCalculator.Application.DTOs.DispenseJobs;
using PaintTintCalculator.Domain.Entities;
using PaintTintCalculator.Domain.Exceptions;

namespace PaintTintCalculator.Application.Features.DispenseJobs.Commands;

public class CreateDispenseJobCommandHandler
{
    private readonly IShadeRepository _shadeRepository;
    private readonly IBaseRepository _baseRepository;
    private readonly ITintCalculationService _tintCalculationService;
    private readonly IDispenseJobRepository _dispenseJobRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDispenseJobCommandHandler(
        IShadeRepository shadeRepository,
        IBaseRepository baseRepository,
        ITintCalculationService tintCalculationService,
        IDispenseJobRepository dispenseJobRepository,
        IUnitOfWork unitOfWork)
    {
        _shadeRepository = shadeRepository;
        _baseRepository = baseRepository;
        _tintCalculationService = tintCalculationService;
        _dispenseJobRepository = dispenseJobRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DispenseJobDto> HandleAsync(CreateDispenseJobRequest request, CancellationToken cancellationToken = default)
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

        // Server recalculates tint strictly - never trusting client figures
        var calculation = _tintCalculationService.Calculate(shade, baseEntity, request.CanSizeLitres);

        var job = new DispenseJob
        {
            ShadeId = shade.Id,
            BaseId = baseEntity.Id,
            CanSizeLitres = calculation.CanSizeLitres,
            TotalColorantMl = calculation.TotalColorantMl,
            TintPercent = calculation.TintPercent,
            TotalPrice = calculation.TotalPrice,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var item in calculation.Items)
        {
            job.Items.Add(new DispenseJobItem
            {
                ColorantId = item.ColorantId,
                DispensedMl = item.DispensedMl,
                Cost = item.TotalCost
            });
        }

        await _dispenseJobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var responseItems = calculation.Items.Select(i => new DispenseJobItemDto(
            0,
            i.ColorantId,
            i.ColorantCode,
            i.ColorantName,
            i.DispensedMl,
            i.TotalCost)).ToList();

        return new DispenseJobDto(
            job.Id,
            shade.Id,
            shade.Code,
            shade.Name,
            baseEntity.Id,
            baseEntity.Name,
            job.CanSizeLitres,
            job.TotalColorantMl,
            job.TintPercent,
            job.TotalPrice,
            job.CreatedAt,
            responseItems);
    }
}

public class GetRecentDispenseJobsQueryHandler
{
    private readonly IDispenseJobRepository _dispenseJobRepository;

    public GetRecentDispenseJobsQueryHandler(IDispenseJobRepository dispenseJobRepository)
    {
        _dispenseJobRepository = dispenseJobRepository;
    }

    public async Task<IReadOnlyList<DispenseJobDto>> HandleAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        var jobs = await _dispenseJobRepository.GetRecentAsync(count, cancellationToken);
        return jobs.Select(j => new DispenseJobDto(
            j.Id,
            j.ShadeId,
            j.Shade.Code,
            j.Shade.Name,
            j.BaseId,
            j.Base.Name,
            j.CanSizeLitres,
            j.TotalColorantMl,
            j.TintPercent,
            j.TotalPrice,
            j.CreatedAt,
            j.Items.Select(i => new DispenseJobItemDto(
                i.Id,
                i.ColorantId,
                i.Colorant.Code,
                i.Colorant.Name,
                i.DispensedMl,
                i.Cost)).ToList()
        )).ToList();
    }
}
