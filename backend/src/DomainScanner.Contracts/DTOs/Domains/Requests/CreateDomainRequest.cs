namespace DomainScanner.Contracts.DTOs.Domains;

public record CreateDomainRequest(
    string Address,
    Guid UserId);