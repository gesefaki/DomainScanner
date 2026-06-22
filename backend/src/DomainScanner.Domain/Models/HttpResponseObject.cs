namespace DomainScanner.Domain.Models;

public class HttpResponseObject
{
    public string Address { get; set; } = string.Empty;
    public ushort StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}