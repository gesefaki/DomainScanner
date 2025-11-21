using DomainScanner.Core.Interfaces;
using DomainScanner.Core.Models;
using DomainScanner.Core.Options;
using Microsoft.Extensions.Options;

namespace DomainScanner.Core.Services;

public class DomainHealthChecker(
    IHttpClientFactory httpClientFactory,
    IOptions<HttpClientOptions> httpOptions,
    IOptions<DomainHealthCheckOptions> healthCheckOptions) : IDomainCheckHealth
{
    private readonly HttpClientOptions _options = httpOptions.Value;
    private readonly DomainHealthCheckOptions _healthCheckOptions = healthCheckOptions.Value;

    public async Task<bool> DomainHealthCheckAsync(Domain domain)
    {
        using var httpClient = httpClientFactory.CreateClient(_healthCheckOptions.HttpClientName);

        try
        {
            using var response = await httpClient.GetAsync(domain.Name);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}