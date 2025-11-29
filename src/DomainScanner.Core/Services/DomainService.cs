using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;

namespace DomainScanner.Core.Services;

public class DomainService(IDomainRepository repo, IHttpClientFabric fabric) : IDomainService
{
    private readonly IDomainRepository _repo = repo;
    private readonly IHttpClientFabric _fabric = fabric;

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
        if (existingDomain == null)
            return;
        
        existingDomain.IsAvailable = status;
        await UpdateAsync(existingDomain.Id, existingDomain);
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
            var response = await http.GetAsync(domain.Name).ConfigureAwait(false);
            status = response.IsSuccessStatusCode;
            return status;
        }
        catch
        {
            status = false;
            return false;
        }
        finally
        {
            await UpdateDomainAvailability(domain, status);
        }

    }
}