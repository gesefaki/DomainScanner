using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.RateLimiting;
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
    [InlineData("Auth", "Login", RateLimitingSettings.Policies.Login)]
    [InlineData("Auth", "Logout", RateLimitingSettings.Policies.Auth)]
    [InlineData("Auth", "GetCsrfToken", RateLimitingSettings.Policies.Auth)]

    // UsersController
    [InlineData("Users", "GetAll", RateLimitingSettings.Policies.Read)]
    [InlineData("Users", "Get", RateLimitingSettings.Policies.Read)]
    [InlineData("Users", "GetMyDomains", RateLimitingSettings.Policies.Read)]
    [InlineData("Users", "Register", RateLimitingSettings.Policies.Auth)]
    [InlineData("Users", "Activate", RateLimitingSettings.Policies.Write)]
    [InlineData("Users", "Deactivate", RateLimitingSettings.Policies.Write)]
    [InlineData("Users", "Delete", RateLimitingSettings.Policies.Write)]

    // DomainsController
    [InlineData("Domains", "Get", RateLimitingSettings.Policies.Read)]
    [InlineData("Domains", "GetHttpCheck", RateLimitingSettings.Policies.Scan)]
    [InlineData(
        "Domains",
        "GetHttpCheckWithDetails",
        RateLimitingSettings.Policies.Scan)]
    [InlineData("Domains", "Update", RateLimitingSettings.Policies.Write)]
    [InlineData("Domains", "Create", RateLimitingSettings.Policies.Write)]
    [InlineData("Domains", "SendAndSave", RateLimitingSettings.Policies.Scan)]
    [InlineData("Domains", "Delete", RateLimitingSettings.Policies.Write)]
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
