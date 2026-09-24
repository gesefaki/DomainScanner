using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.Models;

namespace DomainScanner.Api.IntegrationTests.Middleware;

public sealed class ApiErrorContractTests
{
    [Fact]
    public async Task UnauthorizedRequest_ReturnsApiError()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateHttpsClient();

        using var response = await client.GetAsync("/api/v1/users/me");
        var error = await response.Content.ReadFromJsonAsync<ApiError>();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("authentication_required", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));
    }

    [Fact]
    public async Task UnknownApiRoute_ReturnsApiError()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateAuthenticatedClient(factory, DomainScannerApiFactory.UserAId);

        using var response = await client.GetAsync("/api/v1/unknown");
        var error = await response.Content.ReadFromJsonAsync<ApiError>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("not_found", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));
    }

    [Fact]
    public async Task InvalidJson_ReturnsValidationApiError()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateHttpsClient();
        using var csrfResponse = await client.GetAsync("/api/v1/auth/csrf");
        using var csrfJson = JsonDocument.Parse(await csrfResponse.Content.ReadAsStringAsync());
        var csrfToken = csrfJson.RootElement.GetProperty("token").GetString();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/register")
        {
            Content = new StringContent("{invalid", Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-CSRF-TOKEN", csrfToken);

        using var response = await client.SendAsync(request);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("validation_failed", error.Code);
        Assert.NotEmpty(error.Errors!);
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));

        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("validation_failed", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task RateLimit_ReturnsApiError()
    {
        await using var factory = new DomainScannerApiFactory();
        using var client = factory.CreateHttpsClient();
        for (var i = 0; i < 5; i++)
        {
            using var allowed = await client.GetAsync("/api/v1/auth/csrf");
            Assert.Equal(HttpStatusCode.OK, allowed.StatusCode);
        }

        using var response = await client.GetAsync("/api/v1/auth/csrf");
        var error = await response.Content.ReadFromJsonAsync<ApiError>();

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("rate_limit_exceeded", error.Code);
        Assert.False(string.IsNullOrWhiteSpace(error.TraceId));
    }
}
