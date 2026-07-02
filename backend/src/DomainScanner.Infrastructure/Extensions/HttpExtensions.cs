using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Infrastructure.Protocols.HTTP;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Infrastructure.Extensions;

public static class HttpExtensions
{
    public static IServiceCollection AddHttpExtensions(this IServiceCollection services)
    {
        services.AddHttpClient<IHttpScanner, HttpService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "DomainScanner/1.0");
        });

        services.AddScoped<IHttpScanner, HttpService>();

        return services;
    }
}