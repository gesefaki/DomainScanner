using System.Diagnostics;
using System.Net.Security;
using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;

namespace DomainScanner.Core.Services;

public class DomainService(IDomainRepository repo, IHttpClientFabric fabric) : IDomainService
{
    private readonly IDomainRepository _repo = repo;
    private readonly IHttpClientFabric _fabric = fabric;
    private readonly Lock _lock = new Lock();

    public async Task<IEnumerable<Domain>> GetAllAsync()
    {
        var domains = await _repo.GetAllAsync();
        return domains;
    }

    // Include tracking
    public async Task<Domain?> GetByIdAsync(int id)
    {
        var domain = await _repo.GetByIdAsync(id);
        return domain ?? null;
    }

    // Didn't include tracking
    public async Task<bool> IsExistsByIdAsync(int id) => await _repo.IsExistsByIdAsync(id);

    public async Task AddAsync(Domain domain)
    {
        await _repo.AddAsync(domain);
    }

    public async Task RemoveAsync(int id) => await _repo.RemoveAsync(id);

    public async Task UpdateAsync(int id, Domain domain)
    {
        var existing = await IsExistsByIdAsync(id);
        if (!existing)
            return;
        
        await _repo.UpdateAsync(domain);
    }

    private async Task UpdateDomainAvailability(Domain domain, bool status)
    {
        var existingDomain = await GetByIdAsync(domain.Id);
        if (existingDomain is null)
            return;
        
        existingDomain.IsAvailable = status;
        await UpdateAsync(existingDomain.Id, existingDomain);
    }
    
    public async Task<DomainHealth?> GetHealthAsync(int id)
    {
        var domain = await _repo.GetByIdAsync(id);
        if (domain is null)
            return null;
        
        var (http, tls) = _fabric.CreateHttpClientNoRedirect();

        try
        {
            var stopwatch = Stopwatch.StartNew(); // Starting timer for get response time
            var response = await http.GetAsync(domain.Name); // Get HTTP response
            stopwatch.Stop(); // Get response time
            var scheme = response.RequestMessage!.RequestUri!.Scheme;


            var redirections = new List<string> { domain.Name };
            var redirectionsCount = 0;
            const int maxRedirections = 10;
            var isRedirected = ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400);

            while (isRedirected || redirectionsCount < maxRedirections)
            {
                var location = response.Headers.Location;
                if (location is null) break;

                if (!location.IsAbsoluteUri)
                {
                    var baseAddress = new Uri(location.AbsoluteUri);
                    location = baseAddress;
                }

                redirections.Add(location.ToString());
                response = await http.GetAsync(location);
                redirectionsCount++;
            }

            // Sending the object
            var result = new DomainHealth
            {
                // Main
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseTime = stopwatch.ElapsedMilliseconds,

                // Content
                ContentType = response.Content.Headers.ContentType!.ToString(),
                ContentLength = (long)response.Content.Headers.ContentLength!,
                Server = response.Headers.Server?.ToString(),
                Headers = response.Content.Headers?.ToDictionary
                (x => x.Key,
                    x => string.Join(",", x.Value)),

                // Redirects
                HasRedirects = isRedirected,
                RedirectsCount = redirections.Count,
                Redirects = redirections,

                // TLS (Only HTTPS)
                IsHttps = scheme == Uri.UriSchemeHttps,
                TlsValid = tls.SslPolicyErrors == SslPolicyErrors.None,
                TlsCertificate = tls.ServerCertificate?.Version.ToString(),
                TlsIssuer = tls.ServerCertificate?.Issuer,
                TlsThumbprint = tls.ServerCertificate?.Thumbprint
            };

            return result;
        }
        //TODO: Can we return DTO only with error messages?
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            return new DomainHealth
            {
                IsSuccess = false,
                StatusCode = 504
            };
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return new DomainHealth()
            {
                IsSuccess = false,
                StatusCode = 500
            };
        }
    }

    public async Task<bool> CheckHealthAsync(int id)
    {
        var domain = await _repo.GetByIdAsync(id);
        if (domain == null)
            return false;

        var http = _fabric.CreateHttpClient();
        var status = false;
        try
        {
            var response = await http.GetAsync(domain.Name);
            status = response.IsSuccessStatusCode;
            return status;
        }
        catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
        {
            status = false;
            return false;
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        finally
        {
            await UpdateDomainAvailability(domain, status);
        }

    }
}
