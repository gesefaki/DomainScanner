extern alias DomainScannerApi;

using System.Text.Json;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using FluentValidation;
using FluentValidation.Results;
using ExceptionHandlerMiddleware =
    DomainScannerApi::DomainScanner.Api.Middleware.ExceptionHandlerMiddleware;

namespace DomainScanner.Api.IntegrationTests.Middleware;

public sealed class ExceptionHandlerMiddlewareTests
{
    [Fact]
    public async Task Invoke_ValidationException_ReturnsApiErrorWithFieldErrors()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlerMiddleware(
            _ => Task.FromException(new ValidationException([
                new ValidationFailure("Address", "Address is required."),
                new ValidationFailure("Address", "Address is invalid.")
            ])),
            NullLogger<ExceptionHandlerMiddleware>.Instance);

        await middleware.Invoke(context);

        context.Response.Body.Position = 0;
        var response = await JsonSerializer.DeserializeAsync<ApiError>(
            context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.NotNull(response);
        Assert.Equal("validation_failed", response.Code);
        Assert.Equal(context.TraceIdentifier, response.TraceId);
        Assert.Equal(["Address is required.", "Address is invalid."], response.Errors!["Address"]);
    }

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
        var response = await JsonSerializer.DeserializeAsync<ApiError>(
            body, new JsonSerializerOptions(JsonSerializerDefaults.Web));

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.NotNull(response);
        Assert.Equal("domain_quota_exceeded", response.Code);
        Assert.Equal("Domain quota exceeded.", response.Message);
        Assert.Equal(context.TraceIdentifier, response.TraceId);
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

        var response = await JsonSerializer.DeserializeAsync<ApiError>(
            context.Response.Body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(response);
        Assert.Equal("login_temporarily_blocked", response.Code);
        Assert.Equal(
            "Login is temporarily blocked. Please try again later.",
            response.Message);
        Assert.Equal(context.TraceIdentifier, response.TraceId);
    }
}
