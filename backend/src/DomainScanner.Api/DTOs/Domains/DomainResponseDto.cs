namespace DomainScanner.Api.DTOs.Domains;

public record DomainResponseDto(Guid Id, 
    string Address, 
    bool? IsAvailable, 
    Guid UserId,
    List<HttpResponseDto> Checks);