namespace DomainScanner.Contracts.DTOs.HTTPs.Responses;

/// <summary>HTTP-specific data for a persisted domain check.</summary>
/// <param name="RequestedAddress">Address configured when the check ran.</param>
/// <param name="FinalAddress">Final address after redirects, if a response was received.</param>
/// <param name="StatusCode">Remote HTTP status, or null when no HTTP response was received.</param>
/// <param name="ResponseTimeMs">Elapsed time until response headers or failure, in milliseconds.</param>
/// <param name="ErrorCode">Stable transport error code, or null when an HTTP response was received.</param>
/// <param name="Redirects">Addresses followed during the check, in order.</param>
/// <param name="Tls">TLS summary, if collected for an HTTPS connection.</param>
public sealed record HttpCheckResponse(
    string RequestedAddress,
    string? FinalAddress,
    int? StatusCode,
    long ResponseTimeMs,
    string? ErrorCode,
    IReadOnlyList<string> Redirects,
    HttpTlsResponse? Tls);

/// <summary>Persisted summary of certificate validation for an HTTPS check.</summary>
/// <param name="HasValidationErrors">Whether TLS certificate validation reported errors.</param>
/// <param name="CertificateExpiresAt">UTC certificate expiration, if available.</param>
public sealed record HttpTlsResponse(
    bool? HasValidationErrors,
    DateTime? CertificateExpiresAt);
