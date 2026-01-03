using System.Net.Sockets;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Infrastructure.Protocols.HttpScanner;

public class HttpScanner(IHttpClientFactory httpFactory) : IHttpScanner
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    public async Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken cancellationToken)
    {
        try
        {
            using var http = _httpFactory.CreateClient();

            var response = await http.GetAsync(address, cancellationToken);
            return new HttpResponseObject()
            {
                StatusCode = (int)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode
            };
        }
        catch (OperationCanceledException)
        {
            return new HttpResponseObject()
            {
                StatusCode = 499,
                IsSuccess = false
            };
        }
        catch (SocketException)
        {
            return new HttpResponseObject()
            {
                StatusCode = 504,
                IsSuccess = false
            };
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}