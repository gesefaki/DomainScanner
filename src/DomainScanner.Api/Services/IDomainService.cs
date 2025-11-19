using DomainScanner.Infrastructure.Models;

namespace DomainScanner.Api.Services;

public interface IDomainService
{
    public List<Domain> GetAll();
    Domain? Get(int id);
    bool CheckHealth(int id);
    void Add(Domain domain);
    void Remove(int id);
    void Update(Domain domain);
}