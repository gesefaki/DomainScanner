using System.Net;
using DomainScanner.Api.IntegrationTests.Helpers;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

/// <summary>
/// Verifies per-user rate limits and the process-wide scan concurrency limit.
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

        // Act
        var action = () => TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: AuthEndpoint,
            permitLimit: 5,
            allowedStatusCode: HttpStatusCode.OK);

        // Assert
        await action();
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

        // Act
        var statusCodes = new List<HttpStatusCode>();

        for (var requestNumber = permitLimit + 2;
             requestNumber <= permitLimit + additionalRejectedRequests + 1;
             requestNumber++)
        {
            using var response = await client.GetAsync(AuthEndpoint);
            statusCodes.Add(response.StatusCode);
        }

        // Assert
        Assert.All(
            statusCodes,
            statusCode => Assert.Equal(
                HttpStatusCode.TooManyRequests,
                statusCode));
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

        // Act
        var action = () => TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: ReadEndpoint,
            permitLimit: 100,
            allowedStatusCode: HttpStatusCode.OK);

        // Assert
        await action();
    }

    [Fact]
    public async Task WritePolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);

        // Act
        var action = () => TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: WriteProbeEndpoint,
            permitLimit: 20,
            allowedStatusCode: HttpStatusCode.NoContent);

        // Assert
        await action();
    }

    [Fact]
    public async Task ScanPolicy_RejectsRequestAboveLimit()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);

        // Act
        var action = () => TestingHelper.AssertAllowedThenRejectedAsync(
            client: client,
            endpoint: ScanProbeEndpoint,
            permitLimit: 15,
            allowedStatusCode: HttpStatusCode.NoContent);

        // Assert
        await action();
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

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
    }

    [Fact]
    public async Task ConcurrencyScan_RejectsFourthConcurrentRequest()
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
            .Range(1, 3)
            .Select(_ => client.GetAsync(endpoint, timeout.Token))
            .ToArray();

        HttpStatusCode rejectedStatusCode;

        try
        {
            // All three requests passed through both concurrency limiters.
            await probe.WaitUntilEnteredAsync(
                expectedCount: 3,
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
            .Range(1, 3)
            .Select(_ => client.GetAsync(endpoint, timeout.Token))
            .ToArray();

        try
        {
            await probe.WaitUntilEnteredAsync(expectedCount: 3, timeout.Token);
        }
        finally
        {
            probe.Release();
        }

        var completedResponse =
            await Task.WhenAll(inFlightRequests);

        try
        {
            // Act
            using var nextResponse =
                await client.GetAsync(endpoint, timeout.Token);

            // Assert
            Assert.All(
                completedResponse,
                response => Assert.Equal(
                    HttpStatusCode.NoContent,
                    response.StatusCode)
            );
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

    [Fact]
    public async Task GlobalConcurrencyScan_IsSharedAcrossUsers_AndReleasesSlots()
    {
        // Arrange
        const string endpoint = "/__tests/rate-limiting/scan-concurrency";
        await using var factory = new DomainScannerApiFactory();
        var probe = factory.Services.GetRequiredService<ScanConcurrencyProbe>();
        // Twenty users avoid exhausting the per-user limit of three.
        var clients = Enumerable.Range(0, 21)
            .Select(_ => factory.CreateAuthenticatedClient(factory, Guid.NewGuid()))
            .ToArray();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var requests = clients.Take(20)
            .Select(client => client.GetAsync(endpoint, timeout.Token)).ToArray();
        HttpStatusCode rejectedStatus;
        HttpStatusCode[] allowedStatuses;
        HttpStatusCode nextStatus;

        // Act
        try
        {
            try
            {
                await probe.WaitUntilEnteredAsync(20, timeout.Token);
                using var rejected = await clients[20].GetAsync(endpoint, timeout.Token);
                rejectedStatus = rejected.StatusCode;
            }
            finally
            {
                probe.Release();
            }

            var responses = await Task.WhenAll(requests);
            try
            {
                allowedStatuses = responses.Select(response => response.StatusCode).ToArray();
            }
            finally
            {
                foreach (var response in responses) response.Dispose();
            }

            using var next = await clients[20].GetAsync(endpoint, timeout.Token);
            nextStatus = next.StatusCode;
        }
        finally
        {
            foreach (var client in clients) client.Dispose();
        }

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedStatus);
        Assert.All(allowedStatuses, status => Assert.Equal(HttpStatusCode.NoContent, status));
        Assert.Equal(HttpStatusCode.NoContent, nextStatus);
    }
}
