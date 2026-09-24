namespace DomainScanner.Domain.Models;

/// <summary>Internal result of one HTTP scan, whether or not a server responded.</summary>
public sealed record HttpScanResult(
    string RequestedAddress,
    string? FinalAddress,
    int? StatusCode,
    long ResponseTimeMs,
    string? ErrorCode,
    IReadOnlyList<string> Redirects,
    TlsFetch? Tls)
{
    /// <summary>Whether the final remote HTTP response was successful.</summary>
    public bool IsSuccess => StatusCode is >= 200 and <= 299 && ErrorCode is null;
}
