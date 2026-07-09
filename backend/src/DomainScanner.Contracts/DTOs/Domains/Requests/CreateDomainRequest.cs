namespace DomainScanner.Contracts.DTOs.Domains.Requests;

/// <summary>
/// Request to create a new DomainEntity.
/// </summary>
/// <param name="Address">Domain URL.</param>
/// <param name="UserId">Unique identifier of the user who will own this domain.</param>
public record CreateDomainRequest(
    string Address,
    Guid UserId);