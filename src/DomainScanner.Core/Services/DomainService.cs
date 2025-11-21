using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;

namespace DomainScanner.Core.Services;

public class DomainService(IDomainRepository repo) : IDomainService
{
    private readonly IDomainRepository _repo = repo;

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

    public void Update(Domain domain)
    {
        var existingDomain =  GetById(domain.Id);
        if  (existingDomain != null)
            _repo.Update(existingDomain);
    }
    

}