namespace DomainScanner.Domain.ValueObjects;

public class HttpResponseObject
{
    public ushort StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}