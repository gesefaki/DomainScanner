using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.Models;

namespace DomainScanner.Infrastructure.Protocols.HTTP;

/// <summary>
/// Provides HTTP/HTTPS scanning services for domain monitoring.
/// Implements <see cref="IHttpScanner"/>. 
/// </summary>
public class HttpService : IHttpScanner
{
    private readonly IHttpClientFactory _httpFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpService"/> class.
    /// </summary>
    /// <param name="httpFactory">The HTTP client factory for creating HttpClient instances.</param>
    public HttpService(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    /// <inheritdoc />
    public async Task<HttpResponseObject> GetHttpResponseAsync(Uri address, CancellationToken ct)
    {
        try
        {
            using var http = _httpFactory.CreateClient("DomainScanner.Basic");

            using var response = await http.GetAsync(address, ct);
            return new HttpResponseObject()
            {
                StatusCode = (ushort)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return new HttpResponseObject()
            {
                Address = address.ToString(),
                StatusCode = 504,
                IsSuccess = false
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or SocketException)
        {
            return new HttpResponseObject()
            {
                Address = address.ToString(),
                StatusCode = 504,
                IsSuccess = false
            };
        }
    }

    /// <inheritdoc />
    public async Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken ct)
    {
        var tls = new TlsFetch();

        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            
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

        using var http = new HttpClient(handler);
        http.Timeout = TimeSpan.FromSeconds(30);
        
        http.DefaultRequestHeaders.UserAgent.ParseAdd("DomainScanner/1.0");

        const int maxRedirections = 5;

        Uri currentAddress = address;
        var redirections = new List<string>();
        var stopwatch = Stopwatch.StartNew();
        
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        
        deadline.CancelAfter(TimeSpan.FromSeconds(30));

        HttpResponseDetails Failure(ushort statusCode, string message)
        {
            return new HttpResponseDetails
            {
                Address = currentAddress.ToString(),
                StatusCode = statusCode,
                IsSuccess = false,
                ResponseTime = stopwatch.ElapsedMilliseconds,
                Redirections = redirections,
                RedirectionsCount = (ushort)redirections.Count,
                ErrorMessage = message,
                Tls = tls
            };
        }

        try
        {
            while (true)
            {
                using var response = await http.GetAsync(
                    currentAddress,
                    HttpCompletionOption.ResponseHeadersRead, // reading only headers
                    deadline.Token); 
                
                var statusCode = (int)response.StatusCode;
                var isRedirect = statusCode is 301 or 302 or 303 or 307 or 308;

                if (isRedirect)
                {
                    var location = response.Headers.Location;

                    if (location is null)
                    {
                        return Failure(
                            (ushort)statusCode,
                            "Redirection response has no location header."); 
                    }

                    if (redirections.Count >= maxRedirections)
                    {
                        return Failure(
                            (ushort)statusCode,
                            "Redirect limit exceeded.");
                    }

                    var nextAddress = new Uri(currentAddress, location);

                    if (nextAddress.Scheme != Uri.UriSchemeHttp &&
                        nextAddress.Scheme != Uri.UriSchemeHttps)
                    {
                        return Failure(
                            (ushort)statusCode,
                            "Unsupported redirect scheme.");
                    }
                    
                    currentAddress = nextAddress;
                    redirections.Add(currentAddress.ToString());
                    
                    continue;
                }

                return new HttpResponseDetails
                {
                    Address = currentAddress.ToString(),
                    StatusCode = (ushort)statusCode,
                    IsSuccess = response.IsSuccessStatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds,
                    Redirections = redirections,
                    RedirectionsCount = (ushort)redirections.Count,
                    ReasonPhrase = response.ReasonPhrase ?? string.Empty,
                    ContentType =
                        response.Content.Headers.ContentType?.ToString()
                        ?? string.Empty,
                    ContentLength = response.Content.Headers.ContentLength,
                    ErrorMessage = response.IsSuccessStatusCode
                        ? null
                        : $"Remote server returned HTTP {statusCode}.",
                    Version = response.Version.ToString(),
                    Tls = tls
                };
                
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failure(504, "HTTP check timed out.");
        }
        catch (Exception ex) when (
            ex is HttpRequestException or SocketException)
        {
            return Failure(502, "Connection or TLS error.");
        }
    }
}