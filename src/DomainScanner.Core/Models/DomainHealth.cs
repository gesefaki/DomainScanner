namespace DomainScanner.Core.Models;

public class DomainHealth
{
    // Основные
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public long ResponseTime { get; set; }
    
    // Контент (если в заголовке не передалось)
    public string ContentType { get; set; }
    public long? ContentLength { get; set; }
    
    // Сервер
    public string? Server { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
    
    // Редиректы 
    public bool HasRedirects { get; set; }
    public int RedirectsCount { get; set; }
    public List<string> Redirects { get; set; }
    
    // TLS (HTTPS) (пока не работает)
    /*
    public bool IsHttps { get; set; }
    public bool? TlsValid { get; set; }
    public DateTime? TlsNotBefore  { get; set; }
    public DateTime? TlsNotAfter  { get; set; }
    public string? TlsProtocol { get; set; }
    */
    
    // Мета
    public DateTime CheckedAt { get; private set; } = DateTime.UtcNow;
}