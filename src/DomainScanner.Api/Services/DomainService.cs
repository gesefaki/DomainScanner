using DomainScanner.Api.DTOs;
using DomainScanner.Infrastructure.Models;
using DomainScanner.Infrastructure.Repository;

namespace DomainScanner.Api.Services;

public class DomainService(IDomainRepository repo, IHttpClientFactory clientFactory) : IDomainService
{
    private readonly IHttpClientFactory _httpClientFactory = clientFactory;
    private readonly IDomainRepository _repo = repo;

    public IEnumerable<DomainDto> GetAllDto()
    {
        var domains = _repo.GetAll();
        return domains.Select(d => new DomainDto
        {
            Id = d.Id,
            Name = d.Name,
            IsAvailable = d.IsAvailable,
        });
    }

    public DomainDto? GetDto(int id)
    {
        var domain = _repo.Get(id);
        
        if (domain == null)
            return null;
        
        return new DomainDto
        {
            Id = domain.Id,
            Name = domain.Name,
            IsAvailable = domain.IsAvailable,
        };
    }

    private Domain? Get(int id)
    {
        var domain = _repo.Get(id);
        return domain ?? null;
    }

    public void Add(CreateDomainDto dto)
    {
        var domain = new Domain
        {
            Name = dto.Name,
        };
        
        _repo.Add(domain);
    }

    public void Remove(int id) => _repo.Remove(id);

    private void Update(Domain domain)
    {
        var existingDomain =  _repo.Get(domain.Id);
        if  (existingDomain != null)
            _repo.Update(existingDomain);
    }
    public void UpdateDto(UpdateDomainDto dto)
    {
        var domain = _repo.Get(dto.Id);
        if (domain == null)
            return;
        
        domain.Name = dto.Name;
        domain.IsAvailable = dto.IsAvailable;
        _repo.Update(domain);
    }

    public void UpdateHealthStatus(int id)
    {
        var domain = Get(id);
        if (domain is null)
            return;
        
        using var http = _httpClientFactory.CreateClient();
        try
        {
            using var response = http.GetAsync(domain.Name!).Result;
            var status = response.IsSuccessStatusCode;
            domain.IsAvailable = status;
        }
        catch (Exception)
        {
            domain.IsAvailable = false;
        }

        Update(domain);
    }

}