using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;

namespace DomainScanner.Core.Services;

public class DomainService(IDomainRepository repo, IHttpClientFabric fabric) : IDomainService
{
    private readonly IDomainRepository _repo = repo;
    private readonly IHttpClientFabric _fabric = fabric;

    public IEnumerable<Domain> GetAll()
    {
        var domains = _repo.GetAll();
        return domains;
    }

    public Domain? GetById(int id)
    {
        var domain = _repo.Get(id);
        return domain ?? null;
    }

    public void Add(Domain domain)
    {
        _repo.Add(domain);
    }

    public void Remove(int id) => _repo.Remove(id);

    public void Update(int id, Domain domain)
    {
        var existingDomain = GetById(id);
        if (existingDomain != null)
            _repo.Update(existingDomain);
    }

    private void UpdateDomainAvailability(Domain domain, bool status)
    {
        var existingDomain = GetById(domain.Id);
        if (existingDomain == null)
            return;
        
        existingDomain.IsAvailable = status;
        Update(domain.Id, existingDomain);
    }

    public async Task<bool> CheckHealthAsync(int id)
    {
        var domain = _repo.Get(id);
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
        catch
        {
            status = false;
            return false;
        }
        finally
        {
            UpdateDomainAvailability(domain, status);
        }

    }
}