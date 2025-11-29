using System.Security.Cryptography.X509Certificates;
using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Options;
using DomainScanner.Core.Models;
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

    public (HttpClient client, TlsCapture tls) CreateHttpClientNoRedirect()
    {
        var tls = new TlsCapture();
        
        var handler = new SocketsHttpHandler()
        {
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = _options.MaxConnectionsPerServer,
            SslOptions =
            {
                RemoteCertificateValidationCallback = (sender, cert, chain, errors) =>
                {
                    if (cert is X509Certificate2 c2)
                        tls.ServerCertificate = c2;

                    tls.CertificateChain = chain;
                    tls.SslPolicyErrors = errors;

                    return true;
                }
            }
        };

        var client = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
        
        client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.UserAgent);
        
        return (client, tls);
    }
}