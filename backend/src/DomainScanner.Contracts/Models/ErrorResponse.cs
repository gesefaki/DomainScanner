namespace DomainScanner.Contracts.Models;

public class ErrorResponse
{
    public int StatusCode { get; init; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}