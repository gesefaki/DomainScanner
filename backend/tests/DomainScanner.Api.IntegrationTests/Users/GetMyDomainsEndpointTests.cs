using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Api.IntegrationTests.Users;

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

    private async Task<List<DomainResponse>> GetMyDomainsAsync(Guid userId)
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                factory.CreateAccessToken(userId));

        var response = await client.GetAsync("/api/v1/users/me/domains");

        response.EnsureSuccessStatusCode();

        return await response.Content
                   .ReadFromJsonAsync<List<DomainResponse>>()
               ?? throw new InvalidOperationException(
                   "The endpoint returned an empty response body.");
    }
}
