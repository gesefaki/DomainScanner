namespace DomainScanner.Contracts.DTOs.HTTPs.Responses;

public record HttpResponse(string Address, int StatusCode, bool IsSuccess, DateTime CreatedAt);