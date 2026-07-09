namespace DomainScanner.Contracts.DTOs.Domains.Requests;

/// <summary>
/// Request to update a new DomainEntity.
/// </summary>
/// <param name="Address">Domain URL.</param>
/// <param name="IsActive">Status of domain availability.</param>
public record UpdateDomainRequest(string Address, bool IsActive);