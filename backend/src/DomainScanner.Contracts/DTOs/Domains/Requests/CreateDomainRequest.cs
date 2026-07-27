namespace DomainScanner.Contracts.DTOs.Domains.Requests;

/// <summary>
/// Request to create a new DomainEntity.
/// </summary>
/// <param name="Address">Domain URL.</param>
public record CreateDomainRequest(string? Address);
