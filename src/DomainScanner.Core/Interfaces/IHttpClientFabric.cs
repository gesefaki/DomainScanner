namespace DomainScanner.Core.Interfaces;

public interface IHttpClientFabric
{
    HttpClient CreateHttpClient();
}