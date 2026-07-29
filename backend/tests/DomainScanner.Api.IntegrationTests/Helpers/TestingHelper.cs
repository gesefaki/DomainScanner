using System.Net;

namespace DomainScanner.Api.IntegrationTests.Helpers;

/// <summary>
/// Provides helper methods for integration tests.
/// </summary>
public static class TestingHelper
{ 
    
    /// <summary>
    /// Sends the allowed number of requests to the specified endpoint
    /// and verifies that subsequent requests are rejected by the rate limiter.
    /// </summary>
    /// <param name="client">
    /// The HTTP client used to send requests.
    /// </param>
    /// <param name="endpoint">
    /// The endpoint to which requests are sent.
    /// </param>
    /// <param name="permitLimit">
    /// The maximum number of requests expected to be allowed.
    /// </param>
    /// <param name="allowedStatusCode">
    /// The expected HTTP status code for allowed requests.
    /// </param>
    public static async Task AssertAllowedThenRejectedAsync(
        HttpClient client,
        string endpoint,
        int permitLimit,
        HttpStatusCode allowedStatusCode
    )
    {
        for (var requestNumber = 1;
             requestNumber <= permitLimit;
             requestNumber++)
        {
            using var response = await client.GetAsync(endpoint);

            await AssertStatusCodeAsync(
                response,
                allowedStatusCode,
                requestNumber);
        }

        using var rejected = await client.GetAsync(endpoint);

        await AssertStatusCodeAsync(
            rejected,
            HttpStatusCode.TooManyRequests,
            permitLimit + 1);
    }

    /// <summary>
    /// Verifies that the HTTP response has the expected status code.
    /// </summary>
    /// <param name="response">
    /// The HTTP response to verify.
    /// </param>
    /// <param name="expected">
    /// The expected HTTP status code.
    /// </param>
    /// <param name="requestNumber">
    /// The sequence number of the request, used to provide additional
    /// context when the assertion fails.
    /// </param>
    public static async Task AssertStatusCodeAsync(
        HttpResponseMessage response,
        HttpStatusCode expected,
        int requestNumber
    )
    {
        if (response.StatusCode == expected)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();

        Assert.Fail(
            $"Request #{requestNumber} expected " +
            $"{(int)expected} {expected}, received " +
            $"{(int)response.StatusCode} {response.StatusCode}. " +
            $"Body: {body}"
        );
    }
}