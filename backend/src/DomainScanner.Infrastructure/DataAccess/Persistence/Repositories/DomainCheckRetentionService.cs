using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;

/// <summary>Applies age and per-domain history limits directly in PostgreSQL.</summary>
public sealed class DomainCheckRetentionService : IDomainCheckRetentionService
{
    private readonly ScannerDbContext _context;

    public DomainCheckRetentionService(ScannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    /// <remarks>Count-based retention orders by creation time descending, then identifier descending.</remarks>
    public async Task PruneAsync(DateTime deleteBefore, int maxResultsPerDomain, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxResultsPerDomain);

        await _context.CheckResults
            .Where(result =>
                result.CreatedAt < deleteBefore)
            .ExecuteDeleteAsync(ct);

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             DELETE FROM "CheckResults"
             WHERE "Id" IN
             (
                 SELECT ranked."Id"
                 FROM
                 (
                     SELECT
                         "Id",
                         ROW_NUMBER() OVER
                         (
                             PARTITION BY "DomainId"
                             ORDER BY
                                 "CreatedAt" DESC,
                                 "Id" DESC
                         ) AS row_number
                     FROM "CheckResults"
                 ) AS ranked
                 WHERE ranked.row_number > {maxResultsPerDomain}
             );
             """,
            ct);
    }
}
