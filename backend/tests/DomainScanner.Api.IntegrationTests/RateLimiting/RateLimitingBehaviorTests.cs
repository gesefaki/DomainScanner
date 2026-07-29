using System.Net;
using DomainScanner.Api.IntegrationTests.Helpers;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

/// <summary>
/// Integration tests for Rate Limiting politics. Tests behavior within a single user account.
/// </summary>
public class RateLimitingBehaviorTests
{
    private const string AuthEndpoint = "/api/v1/auth/csrf";
    private const string ReadEndpoint = "/api/v1/users/me/domains";
    private const string AnotherReadEndpoint = "api/v1/users/me";

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

    [Fact]
    public async Task AuthPolicy_ContinuesRejectingRequestsAfterLimitExceeded()
    {
        // Arrange
        const int permitLimit = 5;
        const int additionalRejectedRequests = 3;

        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateHttpsClient();

        await TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: AuthEndpoint,
            permitLimit: permitLimit,
            allowedStatusCode: HttpStatusCode.OK);

        // Act + Assert
        for (var requestNumber = permitLimit + 2;
             requestNumber <= permitLimit + additionalRejectedRequests + 1;
             requestNumber++)
        {
            using var response = await client.GetAsync(AuthEndpoint);

            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.TooManyRequests,
                requestNumber: requestNumber);
        }
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

    [Fact]
    public async Task ReadPolicy_RejectRequestIfGeneralQuotaExhaustForAnyEndpoint()
    {
        // Arrange
        const int requestsCountForFirstEndpoint = 60;
        const int requestsCountForSecondEndpoint = 40;
        HttpResponseMessage response;

        await using var factory = new DomainScannerApiFactory();

        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);

        // Act
        // First endpoint should return 200
        for (var requestNumber = 1; requestNumber <= requestsCountForFirstEndpoint; requestNumber++)
        {
            response = await client.GetAsync(ReadEndpoint);
            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber
            );
        }

        // Second endpoint should return 200
        for (var requestNumber = 1; requestNumber <= requestsCountForSecondEndpoint; requestNumber++)
        {
            response = await client.GetAsync(ReadEndpoint);
            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber
            );
        }

        // Next request after quota exhausting should return 429
        response = await client.GetAsync(AnotherReadEndpoint);

        // Arrange
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrencyScan_RejectsSixthConcurrentRequest()
    {
        // Arrange
        const string endpoint =
            "/__tests/rate-limiting/scan-concurrency";

        await using var factory = new DomainScannerApiFactory();

        var probe =
            factory.Services.GetRequiredService<ScanConcurrencyProbe>();

        using var client = factory.CreateAuthenticatedClient(
            factory,
            DomainScannerApiFactory.UserAId);

        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var inFlightRequests = Enumerable
            .Range(1, 5)
            .Select(_ => client.GetAsync(endpoint, timeout.Token))
            .ToArray();

        HttpStatusCode rejectedStatusCode;

        try
        {
            // All five requests passed through both limiters and were processed.
            await probe.WaitUntilEnteredAsync(
                expectedCount: 5,
                timeout.Token);

            // Act
            using var rejectedResponse =
                await client.GetAsync(endpoint, timeout.Token);

            rejectedStatusCode = rejectedResponse.StatusCode;
        }
        finally
        {
            probe.Release();
        }

        var allowedResponses =
            await Task.WhenAll(inFlightRequests);

        try
        {
            // Assert
            Assert.Equal(
                HttpStatusCode.TooManyRequests,
                rejectedStatusCode);

            Assert.All(
                allowedResponses,
                response => Assert.Equal(
                    HttpStatusCode.NoContent,
                    response.StatusCode));
        }
        finally
        {
            foreach (var response in allowedResponses)
            {
                response.Dispose();
            }
        }
    }
    
    [Fact]
    public async Task ConcurrencyScan_ReleasesSlotsAfterRequestsComplete()
    {
        // Arrange
        const string endpoint = "/__tests/rate-limiting/scan-concurrency";

        await using var factory = new DomainScannerApiFactory();

        var probe =
            factory.Services.GetRequiredService<ScanConcurrencyProbe>();

        using var client
            = factory.CreateAuthenticatedClient(
                factory,
                DomainScannerApiFactory.UserAId);

        using var timeout =
            new CancellationTokenSource(TimeSpan.FromSeconds(5));

        var inFlightRequests = Enumerable
            .Range(1, 5)
            .Select(_ => client.GetAsync(endpoint, timeout.Token))
            .ToArray();

        await probe.WaitUntilEnteredAsync(
            expectedCount: 5,
            timeout.Token);
        
        probe.Release();

        var completedResponse =
            await Task.WhenAll(inFlightRequests);

        try
        {
            Assert.All(
                completedResponse,
                response => Assert.Equal(
                    HttpStatusCode.NoContent,
                    response.StatusCode)
            );

            // Act: the previous five have already been granted concurrency permits.
            using var nextResponse =
                await client.GetAsync(endpoint, timeout.Token);

            // Assert
            Assert.Equal(
                HttpStatusCode.NoContent,
                nextResponse.StatusCode);
        }
        finally
        {
            foreach (var response in completedResponse)
            {
                response.Dispose();
            }
        }
    }
}