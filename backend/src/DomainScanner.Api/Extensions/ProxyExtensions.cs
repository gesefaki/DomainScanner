using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace DomainScanner.Api.Extensions;

public static class ProxyExtensions
{
    public static IServiceCollection AddProxy(this IServiceCollection services,
        IConfiguration configuration)
    {
        var proxyEnabled = configuration.GetValue<bool>("ReverseProxy:Enabled");

        if (!proxyEnabled)
        {
            return services;
        }
        
        var trustedProxy =
            configuration["ReverseProxy:TrustedProxy"]
            ?? throw new InvalidOperationException(
                "Trusted reverse proxy is not configured.");

        if (!IPAddress.TryParse(trustedProxy, out var proxyIp))
        {
            throw new InvalidOperationException(
                "Trusted reverse proxy has an invalid IP address.");
        }

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;

            options.ForwardLimit =
                configuration.GetValue<int>(
                    "ReverseProxy:ForwardLimit");

            options.RequireHeaderSymmetry = true;
            options.KnownProxies.Add(proxyIp);
        });
        
        return services;
    }
}