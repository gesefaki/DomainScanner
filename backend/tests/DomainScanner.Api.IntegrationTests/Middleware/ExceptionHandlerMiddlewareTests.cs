extern alias DomainScannerApi;

using System.Text.Json;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using ExceptionHandlerMiddleware =
    DomainScannerApi::DomainScanner.Api.Middleware.ExceptionHandlerMiddleware;

namespace DomainScanner.Api.IntegrationTests.Middleware;

public sealed class ExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task Invoke_LoginTemporarilyBlocked_Returns429WithRetryAfter()
    {
        // Arrange
        var context = new DefaultHttpContext
        {
            Response =
            {
                Body = new MemoryStream()
            }
        };

        var middleware = new ExceptionHandlerMiddleware(
            _ => Task.FromException(
                new LoginTemporarilyBlockedException(
                    TimeSpan.FromMilliseconds(12_100))),
            NullLogger<ExceptionHandlerMiddleware>.Instance);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.Equal(
            StatusCodes.Status429TooManyRequests,
            context.Response.StatusCode);
        Assert.Equal("13", context.Response.Headers.RetryAfter);

        context.Response.Body.Position = 0;

        var response = await JsonSerializer.DeserializeAsync<ErrorResponse>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(response);
        Assert.Equal(StatusCodes.Status429TooManyRequests, response.StatusCode);
        Assert.Equal(
            "Login is temporarily blocked. Please try again later.",
            response.Message);
    }
}
