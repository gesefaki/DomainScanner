using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.Models;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>Provides deterministic HTTP check results for API integration tests.</summary>
internal sealed class TestHttpScanner : IHttpScanner
{
    /// <inheritdoc />
    public Task<HttpScanResult> CheckAsync(Uri address, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(new HttpScanResult(
            address.ToString(),
            address.ToString(),
            200,
            12,
            null,
            [],
            null));
    }
}
