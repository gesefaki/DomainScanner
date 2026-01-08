using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Api.DTOs.Domains;

public class HttpResponseDetailsDto
{
    public string Address { get; set; } = string.Empty;
    public ushort StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public long ResponseTime { get; set; }
    public string ReasonPhrase { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public uint ContentLength { get; set; }
    public Dictionary<string, string> Headers { get; set;  } = new Dictionary<string, string>();
    public string? ErrorMessage { get; set; }
    public string Version { get; set; } = string.Empty;
    public TlsFetch? Tls { get; set; }
}