using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

/// <summary>Queries domains eligible for scheduled monitoring.</summary>
public interface IDomainScanRepository
{
    /// <summary>Returns a bounded set of monitored domains owned by active users.</summary>
    /// <param name="limit">The positive maximum number of identifiers to return.</param>
    /// <param name="ct">The cancellation token for the query.</param>
    /// <returns>Identifiers ordered by last update (or creation), then identifier, ascending.</returns>
    /// <remarks>Measured availability does not affect eligibility. Check history is not loaded.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The limit is not positive.</exception>
    Task<IReadOnlyList<Guid>> GetMonitorableIdsAsync(
        int limit,
        CancellationToken ct);
    
    /// <summary>Rechecks monitoring eligibility and loads a tracked domain without check history.</summary>
    /// <param name="domainId">The domain identifier.</param>
    /// <param name="ct">The cancellation token for the query.</param>
    /// <returns>The domain, or null if missing, monitoring is disabled, or its owner is inactive or missing.</returns>
    Task<DomainEntity?> GetForScanAsync(
        Guid domainId, 
        CancellationToken ct); 
}
