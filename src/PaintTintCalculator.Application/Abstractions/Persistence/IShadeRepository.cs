using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Application.Abstractions.Persistence;

public interface IShadeRepository
{
    Task<IReadOnlyList<Shade>> SearchAsync(string? search, CancellationToken cancellationToken = default);
    Task<Shade?> GetByIdWithFormulaAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Shade>> GetAllAsync(CancellationToken cancellationToken = default);
}
