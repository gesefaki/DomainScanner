using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Abstractions.Scanners;

/// <summary>Runs bounded HTTP checks without persisting their results.</summary>
public interface IHttpScanner
{
    /// <summary>Checks an HTTP or HTTPS address and returns a transport-aware result.</summary>
    /// <param name="address">Requested address.</param>
    /// <param name="ct">Cancellation requested by the caller.</param>
    Task<HttpScanResult> CheckAsync(Uri address, CancellationToken ct);
}
