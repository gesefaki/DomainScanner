using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.Models;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides deterministic HTTP scan responses for API integration tests.
/// </summary>
internal sealed class TestHttpScanner : IHttpScanner
{
    /// <inheritdoc />
    public Task<HttpResponseObject> GetHttpResponseAsync(
        Uri address,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(new HttpResponseObject
        {
            Address = address.ToString(),
            StatusCode = 200,
            IsSuccess = true
        });
    }

    /// <inheritdoc />
    public Task<HttpResponseDetails> GetHttpWithDetailsAsync(
        Uri address,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        return Task.FromResult(new HttpResponseDetails
        {
            Address = address.ToString(),
            StatusCode = 200,
            IsSuccess = true
        });
    }
}
