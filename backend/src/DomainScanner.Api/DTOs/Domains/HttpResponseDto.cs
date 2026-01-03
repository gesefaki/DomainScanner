namespace DomainScanner.Api.DTOs.Domains;

public record HttpResponseDto(string Address, int StatusCode, bool IsSuccess, DateTime CreateAt);