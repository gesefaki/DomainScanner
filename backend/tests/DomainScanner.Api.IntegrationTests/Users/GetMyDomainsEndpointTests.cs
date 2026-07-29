using System.Net;
using System.Net.Http.Json;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Api.IntegrationTests.Users;

/// <summary>
/// Contains integration tests for the endpoint that returns
/// domains owned by the currently authenticated user.
/// </summary>
/// <param name="factory">
/// The application factory used to create test HTTP clients.
/// </param>
public sealed class GetMyDomainsEndpointTests(
    DomainScannerApiFactory factory)
    : IClassFixture<DomainScannerApiFactory>
{
    [Fact]
    public async Task GetMyDomains_WithoutAccessToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users/me/domains");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyDomains_ReturnsOnlyDomainsOwnedByJwtSubject()
    {
        var domainsOfUserA = await GetMyDomainsAsync(
            DomainScannerApiFactory.UserAId);

        var domainsOfUserB = await GetMyDomainsAsync(
            DomainScannerApiFactory.UserBId);

        Assert.Equal(2, domainsOfUserA.Count);
        Assert.All(
            domainsOfUserA,
            domain => Assert.Equal(
                DomainScannerApiFactory.UserAId,
                domain.UserId));

        Assert.Single(domainsOfUserB);
        Assert.All(
            domainsOfUserB,
            domain => Assert.Equal(
                DomainScannerApiFactory.UserBId,
                domain.UserId));

        Assert.DoesNotContain(
            domainsOfUserA,
            domain => domainsOfUserB.Any(other => other.Id == domain.Id));
    }

    /// <summary>
    /// Requests the domains owned by the specified authenticated test user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user to authenticate as.
    /// </param>
    /// <returns>
    /// A list of domains returned for the specified user.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the endpoint returns an empty response body.
    /// </exception>
    private async Task<List<DomainResponse>> GetMyDomainsAsync(Guid userId)
    {
        using var client = factory.CreateAuthenticatedClient(factory, userId);

        var response = await client.GetAsync("/api/v1/users/me/domains");

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<List<DomainResponse>>()
               ?? throw new InvalidOperationException(
                   "The endpoint returned an empty response body.");
    }
}
