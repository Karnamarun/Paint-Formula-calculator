using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Application.DTOs.Shades;

namespace PaintTintCalculator.Application.Features.Shades.Queries;

public class SearchShadesQueryHandler
{
    private readonly IShadeRepository _shadeRepository;

    public SearchShadesQueryHandler(IShadeRepository shadeRepository)
    {
        _shadeRepository = shadeRepository;
    }

    public async Task<IReadOnlyList<ShadeSummaryDto>> HandleAsync(string? search, CancellationToken cancellationToken = default)
    {
        var shades = await _shadeRepository.SearchAsync(search, cancellationToken);
        return shades.Select(s => new ShadeSummaryDto(s.Id, s.Code, s.Name, s.HexColor)).ToList();
    }
}

public class GetShadeDetailsQueryHandler
{
    private readonly IShadeRepository _shadeRepository;

    public GetShadeDetailsQueryHandler(IShadeRepository shadeRepository)
    {
        _shadeRepository = shadeRepository;
    }

    public async Task<ShadeDetailsDto?> HandleAsync(int id, CancellationToken cancellationToken = default)
    {
        var shade = await _shadeRepository.GetByIdWithFormulaAsync(id, cancellationToken);
        if (shade == null)
        {
            return null;
        }

        var formulas = shade.FormulaItems.Select(f => new FormulaItemDto(
            f.ColorantId,
            f.Colorant.Code,
            f.Colorant.Name,
            f.MlPerLitre,
            f.Colorant.CostPerMl)).ToList();

        return new ShadeDetailsDto(shade.Id, shade.Code, shade.Name, shade.HexColor, formulas);
    }
}

public class GetBasesQueryHandler
{
    private readonly IBaseRepository _baseRepository;

    public GetBasesQueryHandler(IBaseRepository baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<IReadOnlyList<BaseDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var bases = await _baseRepository.GetAllAsync(cancellationToken);
        return bases.Select(b => new BaseDto(b.Id, b.Name, b.MaxTintPercent, b.PricePerLitre)).ToList();
    }
}
