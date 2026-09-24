using System.Diagnostics;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.HTTP;
using DomainScanner.Domain.Models;

namespace DomainScanner.Infrastructure.Protocols.HTTP;

/// <summary>Runs HTTP checks with public-network validation and a whole-check deadline.</summary>
public sealed class HttpService : IHttpScanner
{
    private const int MaxRedirections = 5;
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    public async Task<HttpScanResult> CheckAsync(Uri address, CancellationToken ct)
    {
        var tls = new TlsFetch();
        var stopwatch = Stopwatch.StartNew();
        var redirects = new List<string>();

        using var handler = PublicNetworkHttpHandler.Create(
            (_, certificate, _, errors) =>
            {
                tls.SslPolicyErrors = errors != SslPolicyErrors.None;
                if (certificate is not null)
                {
                    using var leaf = new X509Certificate2(certificate);
                    tls.CertificateExpiresAt = leaf.NotAfter.ToUniversalTime();
                }
                return errors == SslPolicyErrors.None;
            });
        using var http = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("DomainScanner/1.0");

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(ct);
        deadline.CancelAfter(CheckTimeout);

        try
        {
            var result = await SendSafeAsync(http, address, redirects, deadline.Token);
            using var response = result.Response;

            return new HttpScanResult(
                address.ToString(),
                result.FinalAddress.ToString(),
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                null,
                redirects.ToArray(),
                result.FinalAddress.Scheme == Uri.UriSchemeHttps &&
                tls.SslPolicyErrors is not null ? tls : null);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            return Failure("timeout");
        }
        catch (UnsafeOutboundDestinationException)
        {
            return Failure("unsafe_destination");
        }
        catch (HttpRequestException ex)
        {
            var code = ContainsUnsafeDestination(ex)
                ? "unsafe_destination"
                : ex.HttpRequestError switch
            {
                HttpRequestError.NameResolutionError => "dns_error",
                HttpRequestError.SecureConnectionError => "tls_error",
                _ => "network_error"
            };
            return Failure(code);
        }
        catch (SocketException)
        {
            return Failure("network_error");
        }

        HttpScanResult Failure(string code) => new(
            address.ToString(),
            null,
            null,
            stopwatch.ElapsedMilliseconds,
            code,
            redirects.ToArray(),
            tls.SslPolicyErrors is not null ? tls : null);
    }

    private static bool ContainsUnsafeDestination(Exception exception)
    {
        for (Exception? current = exception; current is not null;
             current = current.InnerException)
        {
            if (current is UnsafeOutboundDestinationException)
                return true;
        }
        return false;
    }

    private static async Task<SafeHttpResult> SendSafeAsync(
        HttpClient http, Uri initialAddress, List<string> redirects,
        CancellationToken ct)
    {
        var currentAddress = initialAddress;

        for (var attempt = 0;; attempt++)
        {
            PublicNetworkHttpHandler.ValidateUri(currentAddress);
            using var request = new HttpRequestMessage(HttpMethod.Get, currentAddress);
            var response = await http.SendAsync(
                request, HttpCompletionOption.ResponseHeadersRead, ct);

            if ((int)response.StatusCode is < 300 or > 399 ||
                attempt >= MaxRedirections || response.Headers.Location is null)
            {
                return new SafeHttpResult(response, currentAddress);
            }

            Uri nextAddress;
            try
            {
                nextAddress = new Uri(currentAddress, response.Headers.Location);
                PublicNetworkHttpHandler.ValidateUri(nextAddress);
            }
            finally
            {
                response.Dispose();
            }

            currentAddress = nextAddress;
            redirects.Add(currentAddress.ToString());
        }
    }

    private sealed record SafeHttpResult(
        HttpResponseMessage Response,
        Uri FinalAddress);
}
