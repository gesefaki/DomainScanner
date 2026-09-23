extern alias DomainScannerApi;

using System.Text.Json;
using DomainScanner.Contracts.Exceptions.Domains;
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
    public async Task Invoke_DomainQuotaExceeded_Returns429WithoutRetryAfter()
    {
        // Arrange
        using var body = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = body;
        var middleware = new ExceptionHandlerMiddleware(
            _ => Task.FromException(new DomainQuotaExceededException(50)),
            NullLogger<ExceptionHandlerMiddleware>.Instance);

        // Act
        await middleware.Invoke(context);
        body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ErrorResponse>(
            body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.NotNull(response);
        Assert.Equal(StatusCodes.Status429TooManyRequests, response.StatusCode);
        Assert.Equal("Domain quota exceeded. Please try again later.", response.Message);
    }

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
