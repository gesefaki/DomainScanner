using DomainScanner.Infrastructure.Models;
using DomainScanner.Infrastructure.Repository;

namespace DomainScanner.Api.Services;

public class DomainService(IDomainRepository repo, IHttpClientFactory clientFactory) : IDomainService
{
    private readonly IHttpClientFactory _httpClientFactory = clientFactory;
    private readonly IDomainRepository _repo = repo;
    public List<Domain> GetAll() => _repo.GetAll();

    public Domain? Get(int id) => _repo.Get(id);

    public void Add(Domain domain) => _repo.Add(domain);

    public void Remove(int id) => _repo.Remove(id);

    public void Update(Domain domain) => _repo.Update(domain);

    public bool CheckHealth(int id)
    {
        var domain = Get(id);
        if (domain is null)
            return false;

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
        return domain.IsAvailable ?? false;
    }

}