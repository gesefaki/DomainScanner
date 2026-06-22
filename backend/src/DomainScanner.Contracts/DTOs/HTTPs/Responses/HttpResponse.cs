namespace DomainScanner.Contracts.DTOs.HTTPs;

public record HttpResponse(string Address, int StatusCode, bool IsSuccess, DateTime CreateAt);