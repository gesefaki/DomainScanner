using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Options;
using Microsoft.Extensions.Options;

namespace DomainScanner.Core.Fabrics;

public class HttpClientFabric : IHttpClientFabric
{
    private readonly HttpClientOptions _options;

    public HttpClientFabric(IOptions<HttpClientOptions> options)
    {
        _options = options.Value;
    }

    public HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = _options.AllowAutoRedirect,
            MaxConnectionsPerServer = _options.MaxConnectionsPerServer,
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
        
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);

        return client;
    }
}