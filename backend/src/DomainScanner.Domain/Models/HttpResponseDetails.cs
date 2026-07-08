namespace DomainScanner.Domain.Models;

/// <summary>
/// An extended model that stores more data about the HTTP(S) response and supports TLS fetching. Inhertis from <see cref="HttpResponseObject"/> 
/// </summary>
public class HttpResponseDetails : HttpResponseObject
{
    /// <summary>
    /// Total time taken to receive the complete HTTP response.
    /// </summary>
    public long ResponseTime { get; set; }

    /// <summary>
    /// Collection of URLs that were followed during the HTTP request.
    /// </summary>
    public List<string> Redirections { get; set; } = [];

    /// <summary>
    /// Total number of redirections that occured during the request.
    /// </summary>
    public ushort RedirectionsCount { get; set; }

    /// <summary>
    /// Reason phrase accosiated with the HTTP status code.
    /// </summary>
    public string ReasonPhrase { get; set; } = string.Empty;

    /// <summary>
    /// Content-Type header value of the HTTP response.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Length of the response content in bytes.
    /// </summary>
    public uint ContentLength { get; set; }

    /// <summary>
    /// Any error message that occured during the HTTP request, or null if no error occurred.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// HTTP version used for the response. Any HTTP versions supported.
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    // ONLY HTTPS

    /// <summary>
    /// TLS/SSL details from HTTPS connections. For HTTP requests without TLS/SSL it will contain default values and should be ignored.
    /// </summary>
    /// <value>
    /// A <see cref="TlsFetch"/> object containing TLS information 
    /// </value>
    public TlsFetch Tls { get; set; } = new TlsFetch();
}