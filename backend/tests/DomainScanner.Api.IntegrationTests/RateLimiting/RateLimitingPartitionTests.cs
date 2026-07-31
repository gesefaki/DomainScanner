using System.Net;
using DomainScanner.Api.IntegrationTests.Helpers;
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

        // Act + Assert
        for (var requestNumber = 1; requestNumber <= 100; requestNumber++)
        {
            using var response =
                await userAClient.GetAsync(ReadEndpoint);

            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber);
        }

        // User A quota should be really exhausted.
        using var userARejected =
            await userAClient.GetAsync(ReadEndpoint);

        await TestingHelper.AssertStatusCodeAsync(
            userARejected,
            HttpStatusCode.TooManyRequests,
            requestNumber: 101);

        // User B quota should be unexhausted.
        using var userBResponse =
            await userBClient.GetAsync(ReadEndpoint);

        await TestingHelper.AssertStatusCodeAsync(
            response: userBResponse,
            expected: HttpStatusCode.OK,
            requestNumber: 1);
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

        HttpResponseMessage response;

        using var authenticatedClient = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);
        using var anonymousClient = factory.CreateHttpsClient();
        
        // Act
        // Sending requests to auth endpoint with IP address as partition key
        for (var requestNumber = 1; requestNumber <= 5; requestNumber++)
        {
            response = await anonymousClient.GetAsync(AuthEndpoint);
            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber
            );
        }

        // Request to auth endpoint with JWT as partition key
        response = await authenticatedClient.GetAsync(AuthEndpoint);
        await TestingHelper.AssertStatusCodeAsync(
            response: response,
            expected: HttpStatusCode.OK,
            requestNumber: 1
        );
        
        // Assert
        // Should be OK. IP quota is independent of JWT.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthPolicy_AnonymousClientsWithSameIp_ShareQuota()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        using var firstAnonymousClient = factory.CreateHttpsClient();
        using var secondAnonymousClient = factory.CreateHttpsClient();
        
        // Act
        for (var requestNumber = 1; requestNumber <= 3; requestNumber++)
        {
            using var response = await firstAnonymousClient.GetAsync(AuthEndpoint);

            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber);
        }

        for (var requestNumber = 4; requestNumber <= 5; requestNumber++)
        {
            using var response = await secondAnonymousClient.GetAsync(AuthEndpoint);

            await TestingHelper.AssertStatusCodeAsync(
                response: response,
                expected: HttpStatusCode.OK,
                requestNumber: requestNumber);
        }

        using var rejectedResponse = await secondAnonymousClient.GetAsync(AuthEndpoint);
        
        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, rejectedResponse.StatusCode);
    }
}