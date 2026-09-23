namespace DomainScanner.Application.Abstractions.Persistence;

/// <summary>Removes expired and excess domain check history.</summary>
public interface IDomainCheckRetentionService
{
    /// <summary>Deletes old results, then retains only the newest allowed results per domain.</summary>
    /// <param name="deleteBefore">Exclusive UTC creation-time cutoff.</param>
    /// <param name="maxResultsPerDomain">The positive maximum retained count per domain.</param>
    /// <param name="ct">The cancellation token for deletion.</param>
    /// <returns>A task representing completion of both deletion operations.</returns>
    /// <remarks>
    /// Deletes are applied immediately without SaveChanges. The two operations are not atomic;
    /// concurrent scans can temporarily exceed the retained count until the next cleanup.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The maximum count is not positive.</exception>
    Task PruneAsync(
        DateTime deleteBefore,
        int maxResultsPerDomain,
        CancellationToken ct);
}
