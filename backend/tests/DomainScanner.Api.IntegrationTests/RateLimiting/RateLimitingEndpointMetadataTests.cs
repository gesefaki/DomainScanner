using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

/// <summary>
/// Integration tests for Rate Limiting politics.
/// Tests API Controllers for compliance with the contract regarding rate limiting. 
/// </summary>
public sealed class RateLimitingEndpointMetadataTests
{
    [Fact]
    public async Task HealthEndpoint_HasRateLimitingDisabled()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        var endpointDataSource =
            factory.Services.GetRequiredService<EndpointDataSource>();

        // Act
        var matchingEndpoints = endpointDataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint =>
                endpoint.RoutePattern.RawText == "/health")
            .ToArray();

        // Assert
        var endpoint = Assert.Single(matchingEndpoints);

        var attribute = endpoint.Metadata
            .GetMetadata<DisableRateLimitingAttribute>();

        Assert.NotNull(attribute);
    }

    [Theory]
    // AuthController
    [InlineData("Auth", "Login", RateLimitingOptions.Login)]
    [InlineData("Auth", "Logout", RateLimitingOptions.Auth)]
    [InlineData("Auth", "GetCsrfToken", RateLimitingOptions.Auth)]

    // UsersController
    [InlineData("Users", "GetAll", RateLimitingOptions.Read)]
    [InlineData("Users", "Get", RateLimitingOptions.Read)]
    [InlineData("Users", "GetMyDomains", RateLimitingOptions.Read)]
    [InlineData("Users", "Register", RateLimitingOptions.Auth)]
    [InlineData("Users", "Activate", RateLimitingOptions.Write)]
    [InlineData("Users", "Deactivate", RateLimitingOptions.Write)]
    [InlineData("Users", "Delete", RateLimitingOptions.Write)]

    // DomainsController
    [InlineData("Domains", "Get", RateLimitingOptions.Read)]
    [InlineData("Domains", "GetHttpCheck", RateLimitingOptions.Scan)]
    [InlineData(
        "Domains",
        "GetHttpCheckWithDetails",
        RateLimitingOptions.Scan)]
    [InlineData("Domains", "Update", RateLimitingOptions.Write)]
    [InlineData("Domains", "Create", RateLimitingOptions.Write)]
    [InlineData("Domains", "SendAndSave", RateLimitingOptions.Scan)]
    [InlineData("Domains", "Delete", RateLimitingOptions.Write)]
    public async Task Endpoint_UsesExpectedRateLimitingPolicy(
        string controllerName,
        string actionName,
        string expectedPolicy)
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        var endpointDataSource = factory.Services.GetRequiredService<EndpointDataSource>();

        var matchingEndpoints = endpointDataSource.Endpoints
            .Where(endpoint =>
            {
                var action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                return action?.ControllerName == controllerName && action.ActionName == actionName;
            })
            .ToArray();

        // Assert
        var endpoint = Assert.Single(matchingEndpoints);

        var attribute = endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>();

        Assert.NotNull(attribute);
        Assert.Equal(expectedPolicy, attribute.PolicyName);
    }

    [Fact]
    public async Task EveryApiController_HasRateLimitingPolicy()
    {
        // Arrange
        await using var factory = new DomainScannerApiFactory();

        var endpointDataSource =
            factory.Services.GetRequiredService<EndpointDataSource>();

        // Act
        var endpointsWithoutRateLimiting = endpointDataSource.Endpoints
            .Select(endpoint => new
            {
                Endpoint = endpoint,
                Action = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>()
            })
            .Where(item =>
                item.Action?.ControllerTypeInfo.Namespace == "DomainScanner.Api.Controllers")
            .Where(item => item.Endpoint
                    .Metadata.GetMetadata<EnableRateLimitingAttribute>()
                is null)
            .Select(item =>
                $"{item.Action!.ControllerName}." +
                $"{item.Action.ActionName}")
            .ToArray();
        
        // Assert
        Assert.Empty(endpointsWithoutRateLimiting);
    }
}
