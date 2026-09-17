using System.Net;
using System.Net.Http.Json;
using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Api.IntegrationTests.Domains;

/// <summary>
/// Contains integration tests for domain ownership enforcement across API endpoints.
/// </summary>
/// <param name="factory">
/// The application factory used to create test HTTP clients.
/// </param>
public sealed class DomainOwnershipEndpointTests(
    DomainScannerApiFactory factory)
    : IClassFixture<DomainScannerApiFactory>
{
    [Fact]
    public async Task DomainOwnedByUserA_WhenRequestedByUserB_ReturnsNotFoundForAllOperations()
    {
        // Arrange
        using var registrationClient = factory.CreateHttpsClient();
        await AddAntiforgeryTokenAsync(registrationClient);

        var userA = await RegisterUserAsync(
            registrationClient,
            "ownership-user-a",
            "ownership-user-a@example.com");

        var userB = await RegisterUserAsync(
            registrationClient,
            "ownership-user-b",
            "ownership-user-b@example.com");

        using var userAClient = factory.CreateAuthenticatedClient(
            factory,
            userA.Id);
        await AddAntiforgeryTokenAsync(userAClient);

        using var createResponse = await userAClient.PostAsJsonAsync(
            "/api/v1/domains",
            new CreateDomainRequest("ownership.example.com"));

        createResponse.EnsureSuccessStatusCode();

        var domain = await createResponse.Content
                         .ReadFromJsonAsync<DomainResponse>()
                     ?? throw new InvalidOperationException(
                         "The domain creation endpoint returned an empty response body.");

        using var userBClient = factory.CreateAuthenticatedClient(
            factory,
            userB.Id);
        await AddAntiforgeryTokenAsync(userBClient);

        var endpoint = $"/api/v1/domains/{domain.Id}";

        // Act
        var responses = new (string Operation, HttpResponseMessage Response)[]
        {
            ("GET", await userBClient.GetAsync(endpoint)),
            ("basic-check", await userBClient.GetAsync(
                $"{endpoint}/http/check")),
            ("detailed-check", await userBClient.GetAsync(
                $"{endpoint}/http/check-details")),
            ("send-save", await userBClient.PostAsync(
                $"{endpoint}/send-save",
                content: null)),
            ("PUT", await userBClient.PutAsJsonAsync(
                endpoint,
                new UpdateDomainRequest(
                    "changed-by-foreign-user.example.com",
                    true))),
            ("DELETE", await userBClient.DeleteAsync(endpoint))
        };

        // Assert
        try
        {
            Assert.Equal(userA.Id, domain.UserId);

            foreach (var (operation, response) in responses)
            {
                Assert.True(
                    response.StatusCode == HttpStatusCode.NotFound,
                    $"{operation} expected 404 NotFound, but received " +
                    $"{(int)response.StatusCode} {response.StatusCode}. " +
                    $"Body: {await response.Content.ReadAsStringAsync()}");
            }
        }
        finally
        {
            foreach (var (_, response) in responses)
            {
                response.Dispose();
            }
        }
    }

    /// <summary>
    /// Registers a user through the public API.
    /// </summary>
    /// <param name="client">The HTTP client used to submit the request.</param>
    /// <param name="username">The unique username of the test user.</param>
    /// <param name="email">The unique email of the test user.</param>
    /// <returns>The registered user returned by the API.</returns>
    private static async Task<UserResponse> RegisterUserAsync(
        HttpClient client,
        string username,
        string email)
    {
        using var response = await client.PostAsJsonAsync(
            "/api/v1/users/register",
            new RegisterUserRequest(
                username,
                email,
                "StrongPassword1"));

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserResponse>()
               ?? throw new InvalidOperationException(
                   "The user registration endpoint returned an empty response body.");
    }

    /// <summary>
    /// Obtains an antiforgery token and adds it to subsequent unsafe requests.
    /// </summary>
    /// <param name="client">The HTTP client to configure.</param>
    private static async Task AddAntiforgeryTokenAsync(HttpClient client)
    {
        using var response = await client.GetAsync("/api/v1/auth/csrf");
        response.EnsureSuccessStatusCode();

        var payload = await response.Content
                          .ReadFromJsonAsync<AntiforgeryTokenResponse>()
                      ?? throw new InvalidOperationException(
                          "The antiforgery endpoint returned an empty response body.");

        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", payload.Token);
    }

    private sealed record AntiforgeryTokenResponse(string Token);
}
