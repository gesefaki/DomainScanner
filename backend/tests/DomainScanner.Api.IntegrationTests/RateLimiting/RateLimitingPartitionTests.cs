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
        using var anonymousClient = factory.CreateClient();

        // Act
        for (var requestNumber = 1; requestNumber <= 6; requestNumber++)
        {
            using var authResponse = await anonymousClient.GetAsync("/api/v1/auth/csrf");
        }

        using var readResponse = await authenticatedClient.GetAsync("/api/v1/users/me/domains");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);
    }
}