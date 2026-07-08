using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Infrastructure.Protocols.HTTP;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Infrastructure.Extensions;

/// <summary>
/// Configurates <see cref="HttpClient"/> centrally for future use.
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Applies configuration options to <see cref="HttpClient"/>. 
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
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