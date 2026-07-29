using System.Net;
using DomainScanner.Api.IntegrationTests.Helpers;
using DomainScanner.Api.IntegrationTests.Infrastructure;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

/// <summary>
/// Integration tests for Rate Limiting politics. Tests behavior within a single user account.
/// </summary>
public class RateLimitingBehaviorTests
{
    private const string AuthEndpoint = "/api/v1/auth/csrf";
    private const string ReadEndpoint = "/api/v1/users/me/domains";
    private const string WriteProbeEndpoint =
        "/__tests/rate-limiting/write";
    private const string ScanProbeEndpoint =
        "/__tests/rate-limiting/scan";
    
    /// <summary>
    /// Must reject the request once the limit for the auth policy is exceeded.
    /// </summary>
    [Fact]
    public async Task AuthPolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateHttpsClient();

        // Act + Assert
        await TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: AuthEndpoint,
            permitLimit: 5,
            allowedStatusCode: HttpStatusCode.OK);
    }

    /// <summary>
    /// Must reject the request once the limit for the read policy is exceeded.
    /// </summary>
    [Fact]
    public async Task ReadPolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);

        // Act + Assert
        await TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: ReadEndpoint,
            permitLimit: 100,
            allowedStatusCode: HttpStatusCode.OK);
    }

    [Fact]
    public async Task WritePolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);
        
        // Act + Assert
        await TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: WriteProbeEndpoint,
            permitLimit: 20,
            allowedStatusCode: HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ScanPolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);
        
        // Act + Assert
        await TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: ScanProbeEndpoint,
            permitLimit: 15,
            allowedStatusCode: HttpStatusCode.NoContent);
    }
}