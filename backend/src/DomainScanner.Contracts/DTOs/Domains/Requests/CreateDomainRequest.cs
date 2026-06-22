namespace DomainScanner.Contracts.DTOs.Domains.Requests;

public record CreateDomainRequest(
    string Address,
    Guid UserId);