namespace DomainScanner.Domain.Models;

/// <summary>Certificate validation facts captured during an HTTPS check.</summary>
public sealed class TlsFetch
{
    /// <summary>Whether certificate validation reported errors.</summary>
    public bool? SslPolicyErrors { get; set; }

    /// <summary>UTC expiration of the leaf certificate, if available.</summary>
    public DateTime? CertificateExpiresAt { get; set; }
}
