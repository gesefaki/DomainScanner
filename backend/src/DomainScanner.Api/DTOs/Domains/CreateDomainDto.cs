namespace DomainScanner.Api.DTOs.Domains;

public record CreateDomainDto(
    string Address,
    Guid UserId);