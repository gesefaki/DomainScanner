using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.HTTP;
using DomainScanner.Domain.Models;

namespace DomainScanner.Infrastructure.Protocols.HTTP;

/// <summary>
/// Provides HTTP/HTTPS scanning services for domain monitoring.
/// Implements <see cref="IHttpScanner"/>. 
/// </summary>
public class HttpService : IHttpScanner
{
    private readonly IHttpClientFactory _httpFactory;
    
    private const int MaxRedirections = 5;

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
            // TODO: add from configuration
            var http = _httpFactory.CreateClient("DomainScanner.Basic");

            var result = await SendSafeAsync(
                http,
                address,
                ct);

            using var response = result.Response;

            return new HttpResponseObject
            {
                Address = result.FinalAddress.ToString(),
                StatusCode = (ushort)response.StatusCode,
                IsSuccess = response.IsSuccessStatusCode,
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failure(address, 504);
        }
        catch (UnsafeOutboundDestinationException)
        {
            return Failure(address, 502);
        }
        catch (HttpRequestException)
        {
            return Failure(address, 502);
        }
        catch (SocketException)
        {
            return Failure(address, 502);
        }
    }

    /// <inheritdoc />
    public async Task<HttpResponseDetails> GetHttpWithDetailsAsync(Uri address, CancellationToken ct)
    {
        var tls = new TlsFetch();

        var handler = PublicNetworkHttpHandler.Create(
            (_, certificate, chain, errors) =>
            {
                tls.Certificate = certificate?.ToString();

                if (chain is not null)
                {
                    tls.Chain = string.Join(
                        '\n',
                        chain.ChainElements
                            .Select(element =>
                                $"Subject: {element.Certificate.Subject}, " +
                                $"Issuer: {element.Certificate.Issuer}, " +
                                $"Thumbprint: {element.Certificate.Thumbprint}, " +
                                $"Valid: {element.Certificate.NotBefore} - " +
                                $"{element.Certificate.NotAfter}"));
                }

                tls.SslPolicyErrors = errors != SslPolicyErrors.None;
                
                return errors == SslPolicyErrors.None;
            });

        using var http = new HttpClient(handler);
        http.Timeout = Timeout.InfiniteTimeSpan;
        
        http.DefaultRequestHeaders.UserAgent.ParseAdd(
            "DomainScanner/1.0");
        
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        
        deadline.CancelAfter(TimeSpan.FromSeconds(30));

        var stopwatch = Stopwatch.StartNew();
        
        var result = await SendSafeAsync(
            http,
            address,
            deadline.Token);
        
        stopwatch.Stop();

        using var response = result.Response;

        return new HttpResponseDetails
        {
            Address = result.FinalAddress.ToString(),
            StatusCode = (ushort)response.StatusCode,
            IsSuccess = response.IsSuccessStatusCode,
            ResponseTime = stopwatch.ElapsedMilliseconds,
            Redirections = result.Redirections,
            RedirectionsCount =
                checked((ushort)result.Redirections.Count),
            ReasonPhrase = response.ReasonPhrase ?? string.Empty,
            ContentType =
                response.Content.Headers.ContentType?.ToString()
                ?? string.Empty,
            ContentLength =
                response.Content.Headers.ContentLength,
            ErrorMessage = response.IsSuccessStatusCode
                ? null
                : $"Remote server returned HTTP {(int)response.StatusCode}.",
            Version = response.Version.ToString(),
            Tls = tls
        };
    }

    private static async Task<SafeHttpResult> SendSafeAsync(
        HttpClient http,
        Uri initAddress,
        CancellationToken ct)
    {
        var currentAddress = initAddress;
        var redirections = new List<string>();

        for (var attempt = 0;; attempt++)
        {
            PublicNetworkHttpHandler.ValidateUri(currentAddress);
            
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                currentAddress);

            var response = await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            if (!IsRedirect((int)response.StatusCode))
            {
                return new SafeHttpResult(
                    response,
                    currentAddress,
                    redirections);
            }

            if (attempt >= MaxRedirections)
            {
                response.Dispose();
                
                throw new HttpRequestException(
                    "Redirect limit exceeded.");
            }
            
            var location = response.Headers.Location;

            if (location is null)
            {
                response.Dispose();

                throw new HttpRequestException(
                    "Redirection response has no location header."
                );
            }

            Uri nextAddress;

            try
            {
                nextAddress = new Uri(currentAddress, location);
                PublicNetworkHttpHandler.ValidateUri(nextAddress);
            }
            catch
            {
                response.Dispose();
                throw;
            }
            
            response.Dispose();
            
            currentAddress = nextAddress;
            redirections.Add(currentAddress.ToString());
        }
    }

    private static bool IsRedirect(int statusCode)
    {
        return statusCode is >= 300 and <= 399;
    }
    
    private sealed record SafeHttpResult(
        HttpResponseMessage Response,
        Uri FinalAddress,
        List<string> Redirections
    );

    private static HttpResponseObject Failure(
        Uri address,
        ushort statusCode)
    {
        return new HttpResponseObject
        {
            Address = address.ToString(),
            StatusCode = statusCode,
            IsSuccess = false
        };
    }
}