using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Infrastructure.Protocols.HTTP;

public class HttpService(IHttpClientFactory httpFactory) : IHttpScanner
{
    private readonly IHttpClientFactory _httpFactory = httpFactory;

    public async Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken ct)
    {
        try
        {
            using var http = _httpFactory.CreateClient();

            using var response = await http.GetAsync(address, ct);
            return new HttpResponseObject()
            {
                StatusCode = (ushort)response.StatusCode,
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
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
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


    public async Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken ct)
    {
        var tls = new TlsFetch();

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            
            ServerCertificateCustomValidationCallback = (message, cert, chain, error) =>
            {
                tls.Message = message.ToString();
                tls.Certificate = cert?.ToString();
                if (chain is not null)
                {
                    var chainElements = new List<string>();
                    foreach (var element in chain.ChainElements)
                    {
                        chainElements
                            .Add($"Subject: {element.Certificate.Subject}, " +
                                 $"Issuer: {element.Certificate.Issuer}, " +
                                 $"Thumbprint: {element.Certificate.Thumbprint}, " +
                                 $"Valid: {element.Certificate.NotBefore} - {element.Certificate.NotAfter}");
                    }
                    tls.Chain = string.Join('\n', chainElements);
                }
                tls.SslPolicyErrors = error != SslPolicyErrors.None;

                return error == SslPolicyErrors.None;
            }
        };
        
        var http = new HttpClient(handler);

        const int maxRedirections = 5;
        int currentRedirections = 0;
        
        var redirections = new List<string>();
        
        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var response = await http.GetAsync(address, ct);
            stopwatch.Stop();
            
            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                while (currentRedirections < maxRedirections)
                {
                    var currentResponse = await http.GetAsync(address, ct);
                    redirections.Add(currentResponse.Headers.Location!.ToString());
                    currentRedirections++;
                }
            }

            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = (ushort)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Redirections = redirections,
                RedirectionsCount = (ushort)currentRedirections,
                ReasonPhrase = response.ReasonPhrase!,
                ContentType = response.Content.Headers.ContentType!.ToString(),
                ContentLength = (uint)response.Content.Headers.ContentLength!,
                ErrorMessage = !response.IsSuccessStatusCode
                    ? await response.Content.ReadAsStringAsync(ct)
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
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            return new HttpResponseDetails()
            {
                Address = address.ToString(),
                StatusCode = 504,
                IsSuccess = false,
                ReasonPhrase = string.Empty,
                ErrorMessage = "Timeout or not found."
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