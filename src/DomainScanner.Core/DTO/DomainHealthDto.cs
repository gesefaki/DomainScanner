namespace DomainScanner.Core.DTO;

public class DomainHealthDto
{
    // Main
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTime { get; set; }
    
    // Content
    public string? ContentType { get; set; }
    public long? ContentLength { get; set; }
    
    // Server
    public string? Server { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    
    // Redirects 
    public bool HasRedirects { get; set; }
    public int RedirectsCount { get; set; }
    public List<string>? Redirects { get; set; }
    
    // TLS (HTTPS) 
    public bool IsHttps { get; set; }
    public bool? TlsValid { get; set; }
    public string? TlsCertificate { get; set; }
    public string? TlsIssuer { get; set; }
    public string? TlsThumbprint { get; set; }
    
    
    // Meta
    public DateTime CheckedAt { get; private set; } = DateTime.UtcNow;
    
}