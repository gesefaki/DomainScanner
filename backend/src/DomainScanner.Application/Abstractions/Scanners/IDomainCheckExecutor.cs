using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Scanners;

/// <summary>
/// Defines the contract for executing HTTP checks and persisting their results.
/// </summary>
public interface IDomainCheckExecutor
{
    /// <summary>
    /// Executes an HTTP check for the specified domain, updates its availability status,
    /// and persists the check result.
    /// </summary>
    /// <param name="domain">The domain to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The persisted domain check result.</returns>
    Task<DomainCheckResult> ExecuteAndSaveAsync(
        DomainEntity domain,
        CancellationToken ct
    );
}
