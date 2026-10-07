using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Application.Abstractions.Persistence;

public interface IBaseRepository
{
    Task<IReadOnlyList<Base>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Base?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
