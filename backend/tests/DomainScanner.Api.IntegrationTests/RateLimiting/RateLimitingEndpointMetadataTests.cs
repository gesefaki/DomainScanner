using DomainScanner.Api.IntegrationTests.Infrastructure;
using DomainScanner.Contracts.Options;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Api.IntegrationTests.RateLimiting;

public sealed class RateLimitingEndpointMetadataTests
{
    [Theory]
    // AuthController
    [InlineData("Auth", "Login", RateLimitingOptions.Auth)]
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
}