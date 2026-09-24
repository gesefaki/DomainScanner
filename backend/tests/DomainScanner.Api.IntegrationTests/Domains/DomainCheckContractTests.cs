using System.Net;
using System.Net.Http.Json;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Api.IntegrationTests.Domains;

/// <summary>Verifies that a saved HTTP check has one shape across write and read endpoints.</summary>
public sealed class DomainCheckContractTests
{
    private static readonly Guid OwnedDomainId =
        Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    [Fact]
    public async Task SavedCheck_HasSameContractInCreateSingleHistoryAndDomainSummary()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(
            factory, DomainScannerApiFactory.UserAId);

        using var csrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        csrfResponse.EnsureSuccessStatusCode();
        var csrf = await csrfResponse.Content.ReadFromJsonAsync<CsrfResponse>();
        Assert.NotNull(csrf);
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.Token);

        var domainPath = $"/api/v1/domains/{OwnedDomainId}";
        using var createdResponse = await client.PostAsync(
            $"{domainPath}/http/checks", null);
        Assert.True(createdResponse.StatusCode == HttpStatusCode.Created,
            await createdResponse.Content.ReadAsStringAsync());
        var created = await createdResponse.Content
            .ReadFromJsonAsync<DomainCheckResponse>();
        Assert.NotNull(created);
        Assert.Equal("http", created.Kind);
        Assert.Equal("up", created.Outcome);
        Assert.Equal(200, created.Http?.StatusCode);
        Assert.Equal(12, created.Http?.ResponseTimeMs);
        Assert.Null(created.Http?.ErrorCode);
        Assert.EndsWith($"{domainPath}/checks/{created.Id}",
            createdResponse.Headers.Location?.ToString());

        using var singleResponse = await client.GetAsync(
            $"{domainPath}/checks/{created.Id}");
        singleResponse.EnsureSuccessStatusCode();
        var single = await singleResponse.Content
            .ReadFromJsonAsync<DomainCheckResponse>();
        Assert.Equal(created.Id, single?.Id);
        Assert.Equal(created.DomainId, single?.DomainId);
        Assert.Equal(created.Outcome, single?.Outcome);
        Assert.Equal(created.Http?.RequestedAddress, single?.Http?.RequestedAddress);
        Assert.Equal(created.Http?.StatusCode, single?.Http?.StatusCode);
        Assert.Equal(created.Http?.ErrorCode, single?.Http?.ErrorCode);

        using var historyResponse = await client.GetAsync($"{domainPath}/checks");
        historyResponse.EnsureSuccessStatusCode();
        var history = await historyResponse.Content
            .ReadFromJsonAsync<DomainCheckResponse[]>();
        Assert.NotNull(history);
        Assert.Contains(history, check =>
            check.Id == created.Id &&
            check.Http?.StatusCode == created.Http?.StatusCode);

        using var domainResponse = await client.GetAsync(domainPath);
        domainResponse.EnsureSuccessStatusCode();
        var domain = await domainResponse.Content
            .ReadFromJsonAsync<DomainResponse>();
        Assert.Equal(created.Id, domain?.LastCheck?.Id);
        Assert.Equal("up", domain?.LastCheck?.Outcome);

        var otherOwnedDomainId =
            Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
        using var wrongDomainResponse = await client.GetAsync(
            $"/api/v1/domains/{otherOwnedDomainId}/checks/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, wrongDomainResponse.StatusCode);
    }

    [Fact]
    public async Task OldProbeRoutes_AreNotPublicEndpoints()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(
            factory, DomainScannerApiFactory.UserAId);
        var path = $"/api/v1/domains/{OwnedDomainId}";

        using var basic = await client.GetAsync($"{path}/http/check");
        using var details = await client.GetAsync($"{path}/http/check-details");

        Assert.Equal(HttpStatusCode.NotFound, basic.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, details.StatusCode);
    }

    private sealed record CsrfResponse(string Token);
}
