namespace DomainScanner.Domain.Models;

/// <summary>
/// Represents the TLS/SSL information fetched during an HTTPS connection.
/// </summary>
public class TlsFetch
{
    /// <summary>
    /// Any error or informational message related to the TLS fetch operation.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Primary TLS/SSL certificate information.
    /// </summary>
    public string? Certificate { get; set; }

    /// <summary>
    /// Certificate chain information including intermediate and root certificates.
    /// </summary>
    public string? Chain { get; set; }

    /// <summary>
    /// Value indicating whether SSL policy validation encountered any errors.
    /// </summary>
    public bool? SslPolicyErrors { get; set; }
}