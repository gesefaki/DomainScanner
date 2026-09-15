namespace DomainScanner.Domain.Models;

/// <summary>
/// An extended model that stores more data about the HTTP(S) response and supports TLS fetching. Inhertis from <see cref="HttpResponseObject"/> 
/// </summary>
public class HttpResponseDetails : HttpResponseObject
{
    /// <summary>
    /// Elapsed time in milliseconds until the final response headers are received, including redirects.
    /// If the check fails, contains the elapsed time until the failure. Response body download time is excluded.
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
    /// Reason phrase accosted with the HTTP status code.
    /// </summary>
    public string ReasonPhrase { get; set; } = string.Empty;

    /// <summary>
    /// Content-Type header value of the HTTP response.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Content-Length header value in bytes, or <see langword="null"/> if the header is absent.
    /// </summary>
    public long? ContentLength { get; set; }

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
