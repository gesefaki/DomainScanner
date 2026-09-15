using DomainScanner.Domain.Models;

namespace DomainScanner.Contracts.DTOs.HTTPs.Responses;
/// <summary>
/// Advanced <c>HttpResponse</c> variant for map <c>Domain.Models.HttpResponseDetails</c> model.
/// </summary>
public class HttpResponseDetails
{
    public string Address { get; set; } = string.Empty;
    public ushort StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Elapsed time in milliseconds until the final response headers are received, including redirects.
    /// If the check fails, contains the elapsed time until the failure. Response body download time is excluded.
    /// </summary>
    public long ResponseTime { get; set; }
    public List<string> Redirections { get; set; } = [];
    public ushort RedirectionsCount { get; set; }
    public string ReasonPhrase { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    /// <summary>
    /// Content-Length header value in bytes, or <see langword="null"/> if the header is absent.
    /// </summary>
    public long? ContentLength { get; set; }
    public Dictionary<string, string> Headers { get; set;  } = new Dictionary<string, string>();
    public string? ErrorMessage { get; set; }
    public string Version { get; set; } = string.Empty;
    public TlsFetch? Tls { get; set; }
}
