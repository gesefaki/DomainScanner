using System.Net;
using DomainScanner.Api.IntegrationTests.Infrastructure;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

/// <summary>
/// Integration tests for Rate Limiting politics. Tests partition within a multiple users accounts.
/// </summary>
public class RateLimitingPartitionTests
{
    private const string ReadEndpoint = "/api/v1/users/me/domains";
    private const string AuthEndpoint = "/api/v1/auth/csrf";
    
    /// <summary>
    /// Must accept the request from B user with unexhausted quota when A user quota is exhausted.
    /// </summary>
    [Fact]
    public async Task ReadPolicy_HasIndependentQuotaForEachUser()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        using var userAClient = factory.CreateAuthenticatedClient(
            factory,
            DomainScannerApiFactory.UserAId);

        using var userBClient = factory.CreateAuthenticatedClient(
            factory,
            DomainScannerApiFactory.UserBId);

        // Act
        var userAStatusCodes = new List<HttpStatusCode>();

        for (var requestNumber = 1; requestNumber <= 100; requestNumber++)
        {
            using var response =
                await userAClient.GetAsync(ReadEndpoint);

            userAStatusCodes.Add(response.StatusCode);
        }

        // User A quota should be really exhausted.
        using var userARejected =
            await userAClient.GetAsync(ReadEndpoint);

        // User B quota should be unexhausted.
        using var userBResponse =
            await userBClient.GetAsync(ReadEndpoint);

        // Assert
        Assert.All(
            userAStatusCodes,
            statusCode => Assert.Equal(HttpStatusCode.OK, statusCode));
        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            userARejected.StatusCode);
        Assert.Equal(HttpStatusCode.OK, userBResponse.StatusCode);
    }

    /// <summary>
    /// Must accept the request to the endpoint with Auth policy when Read policy quota is exhausted.
    /// </summary>
    [Fact]
    public async Task ExhaustedAuthPolicy_DoesNotConsumeReadQuota()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        using var authenticatedClient = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);
        using var anonymousClient = factory.CreateHttpsClient();

        // Act
        for (var requestNumber = 1; requestNumber <= 6; requestNumber++)
        {
            using var authResponse = await anonymousClient.GetAsync(AuthEndpoint);
        }

        using var readResponse = await authenticatedClient.GetAsync(ReadEndpoint);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
    }

    [Fact]
    public async Task AuthPolicy_ExhaustQuotaWithIP_ShouldNotRejectNextRequestForAuthEndpointWithJWT()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        using var authenticatedClient = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);
        using var anonymousClient = factory.CreateHttpsClient();
        
        // Act
        // Sending requests to auth endpoint with IP address as partition key
        var anonymousStatusCodes = new List<HttpStatusCode>();

        for (var requestNumber = 1; requestNumber <= 5; requestNumber++)
        {
            using var response = await anonymousClient.GetAsync(AuthEndpoint);
            anonymousStatusCodes.Add(response.StatusCode);
        }

        // Request to auth endpoint with JWT as partition key
        using var authenticatedResponse =
            await authenticatedClient.GetAsync(AuthEndpoint);
        
        // Assert
        // Should be OK. IP quota is independent of JWT.
        Assert.All(
            anonymousStatusCodes,
            statusCode => Assert.Equal(HttpStatusCode.OK, statusCode));
        Assert.Equal(HttpStatusCode.OK, authenticatedResponse.StatusCode);
    }

    [Fact]
    public async Task AuthPolicy_AnonymousClientsWithSameIp_ShareQuota()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        using var firstAnonymousClient = factory.CreateHttpsClient();
        using var secondAnonymousClient = factory.CreateHttpsClient();
        
        // Act
        var allowedStatusCodes = new List<HttpStatusCode>();

        for (var requestNumber = 1; requestNumber <= 3; requestNumber++)
        {
            using var response = await firstAnonymousClient.GetAsync(AuthEndpoint);
            allowedStatusCodes.Add(response.StatusCode);
        }

        for (var requestNumber = 4; requestNumber <= 5; requestNumber++)
        {
            using var response = await secondAnonymousClient.GetAsync(AuthEndpoint);
            allowedStatusCodes.Add(response.StatusCode);
        }

        using var rejectedResponse = await secondAnonymousClient.GetAsync(AuthEndpoint);
        
        // Assert
        Assert.All(
            allowedStatusCodes,
            statusCode => Assert.Equal(HttpStatusCode.OK, statusCode));
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
    }
}
