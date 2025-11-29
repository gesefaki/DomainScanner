using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IHttpClientFabric
{
    HttpClient CreateHttpClient();
    (HttpClient client, TlsCapture tls) CreateHttpClientNoRedirect();
}