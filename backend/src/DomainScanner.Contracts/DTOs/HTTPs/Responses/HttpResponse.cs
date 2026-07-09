namespace DomainScanner.Contracts.DTOs.HTTPs.Responses;

/// <summary>
/// Basic response model for map <c>Domain.Models.HttpResponseObject</c>
/// </summary>
/// <param name="Address">HTTP request address.</param>
/// <param name="StatusCode">HTTP Status code.</param>
/// <param name="IsSuccess">Indicates whether the HTTP request was successful (2xx status code).</param>
/// <param name="CreatedAt">The timestamp when the HTTP check was perform.</param>
public record HttpResponse(string Address, int StatusCode, bool IsSuccess, DateTime CreatedAt);