using DomainScanner.Domain.Models;

namespace DomainScanner.Contracts.DTOs.HTTPs.Responses;

public class HttpResponseDetails
{
    public string Address { get; set; } = string.Empty;
    public ushort StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public long ResponseTime { get; set; }
    public List<string> Redirections { get; set; } = new List<string>();
    public ushort RedirectionsCount { get; set; }
    public string ReasonPhrase { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public uint ContentLength { get; set; }
    public Dictionary<string, string> Headers { get; set;  } = new Dictionary<string, string>();
    public string? ErrorMessage { get; set; }
    public string Version { get; set; } = string.Empty;
    public TlsFetch? Tls { get; set; }
}