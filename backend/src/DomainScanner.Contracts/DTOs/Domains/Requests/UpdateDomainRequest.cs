namespace DomainScanner.Contracts.DTOs.Domains;

public record UpdateDomainRequest(string Address, bool IsAvailable);