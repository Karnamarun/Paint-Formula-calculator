using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Application.Abstractions.Persistence;

public interface IDispenseJobRepository
{
    Task AddAsync(DispenseJob job, CancellationToken cancellationToken = default);
    Task<DispenseJob?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DispenseJob>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default);
}

