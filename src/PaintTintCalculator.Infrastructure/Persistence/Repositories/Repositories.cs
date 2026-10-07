using Microsoft.EntityFrameworkCore;
using PaintTintCalculator.Application.Abstractions.Persistence;
using PaintTintCalculator.Domain.Entities;

namespace PaintTintCalculator.Infrastructure.Persistence.Repositories;

public class ShadeRepository : IShadeRepository
{
    private readonly PaintTintDbContext _context;

    public ShadeRepository(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Shade>> SearchAsync(string? search, CancellationToken cancellationToken = default)
    {
        var query = _context.Shades.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term));
        }

        return await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public async Task<Shade?> GetByIdWithFormulaAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Shades
            .Include(s => s.FormulaItems)
                .ThenInclude(f => f.Colorant)
            .Include(s => s.FormulaItems)
                .ThenInclude(f => f.Base)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Shade>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Shades
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}

public class BaseRepository : IBaseRepository
{
    private readonly PaintTintDbContext _context;

    public BaseRepository(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Base>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Bases
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Base?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Bases
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}

public class DispenseJobRepository : IDispenseJobRepository
{
    private readonly PaintTintDbContext _context;

    public DispenseJobRepository(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DispenseJob job, CancellationToken cancellationToken = default)
    {
        await _context.DispenseJobs.AddAsync(job, cancellationToken);
    }

    public async Task<DispenseJob?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.DispenseJobs
            .Include(j => j.Shade)
            .Include(j => j.Base)
            .Include(j => j.Items)
                .ThenInclude(i => i.Colorant)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<DispenseJob>> GetRecentAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return await _context.DispenseJobs
            .AsNoTracking()
            .Include(j => j.Shade)
            .Include(j => j.Base)
            .Include(j => j.Items)
                .ThenInclude(i => i.Colorant)
            .OrderByDescending(j => j.Id)
            .Take(count)
            .ToListAsync(cancellationToken);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly PaintTintDbContext _context;

    public UnitOfWork(PaintTintDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
