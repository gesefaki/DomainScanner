using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Auth;

/// <summary>
/// Defines the contract for retrieving domains owned by the current authenticated user.
/// </summary>
public interface IOwnedDomainProvider
{
    /// <summary>
    /// Retrieves a domain by its identifier when it belongs to the current authenticated user.
    /// </summary>
    /// <param name="domainId">Unique identifier of the domain to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The domain owned by the current authenticated user.</returns>
    Task<DomainEntity> GetRequiredAsync(
        Guid domainId,
        CancellationToken ct
    );
}
