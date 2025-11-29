namespace DomainScanner.Core.Models;

public class DomainHealth
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTime { get; set; }
    
    public long? ContentLength { get; set; }
    public string? ContentType { get; set; }
    
    public string Server { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    
    public bool HasRedirects { get; set; }
    public int RedirectsCount { get; set; }
    public List<string> Redirects { get; set; }
    
    public bool IsHttps { get; set; }
    public bool TlsValid { get; set; }
    public DateTime? TlsNotBefore  { get; set; }
    public DateTime? TlsNotAfter  { get; set; }
    public string TlsProtocol { get; set; }
    
    public string? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }

    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}