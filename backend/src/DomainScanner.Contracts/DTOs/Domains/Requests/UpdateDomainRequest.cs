namespace DomainScanner.Contracts.DTOs.Domains.Requests;

public record UpdateDomainRequest(string Address, bool IsActive);