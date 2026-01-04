using System.Net.Security;
using System.Net.Sockets;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Infrastructure.Protocols.HttpService;

public class HttpService(IHttpClientFactory httpFactory) : IHttpScanner
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    public async Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken cancellationToken)
    {
        try
        {
            using var http = _httpFactory.CreateClient();

            using var response = await http.GetAsync(address, cancellationToken);
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

    public async Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken cancellationToken)
    {
        var tls = new TlsFetch();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, error) =>
            {
                tls.Message = message.ToString();
                tls.Certificate = cert?.ToString();
                tls.Chain = chain?.ChainElements.ToString();
                tls.SslPolicyErrors = error != SslPolicyErrors.None;

                return error == SslPolicyErrors.None;
            }
        };
        
        var http = new HttpClient(handler);
        
        try
        {
            using var response = await http.GetAsync(address, cancellationToken);
            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = (ushort)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                ReasonPhrase = response.ReasonPhrase!,
                ContentType = response.Content.Headers.ContentType!.ToString(),
                ContentLength = (uint)response.Content.Headers.ContentLength!,
                Headers = response.Headers.ToDictionary(h
                    => h.Key, h => string.Join(",", h.Value)),
                ErrorMessage = !response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync(cancellationToken)
                    : null,
                Version = response.Version.ToString(),
                Tls = tls
            };
        }
        catch (OperationCanceledException)
        {
            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = 499,
                IsSuccess = false,
                ReasonPhrase = string.Empty,
                ErrorMessage = "Operation was canceled"
            };
        }
        catch (SocketException)
        {
            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = 504,
                IsSuccess = false,
                ReasonPhrase = string.Empty,
                ErrorMessage = "Socket was closed"
            };
        }
        catch (Exception)
        {
            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = 500,
                IsSuccess = false,
                ReasonPhrase = string.Empty,
                ErrorMessage = "Internal server error. Please try again later."
            };
        }
    }
}