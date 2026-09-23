using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;

/// <summary>EF Core queries for monitored domains belonging to active users, without auto-included history.</summary>
public class DomainScanRepository : IDomainScanRepository
{
    private readonly ScannerDbContext _context;

    public DomainScanRepository(ScannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Guid>> GetMonitorableIdsAsync(int limit, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);

        return await _context.Domains
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(d =>
                d.MonitoringEnabled &&
                d.User != null &&
                d.User.IsActive)
            .OrderBy(d =>
                d.UpdatedAt ?? d.CreatedAt)
            .ThenBy(d => d.Id)
            .Take(limit)
            .Select(d => d.Id)
            .ToListAsync(ct);
    }
    
    /// <inheritdoc />
    public async Task<DomainEntity?> GetForScanAsync(Guid domainId, CancellationToken ct)
    {
        return await _context.Domains
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(
                d =>
                    d.Id == domainId &&
                    d.MonitoringEnabled &&
                    d.User != null &&
                    d.User.IsActive,
                ct
            );
    }
}
