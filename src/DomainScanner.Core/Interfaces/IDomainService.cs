using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainService
{
    public IEnumerable<Domain> GetAll();
    public Domain? GetById(int id);
    public bool IsExistsById(int id);
    public void Add(Domain domain);
    public void Remove(int id);
    public void Update(int id, Domain domain);
    public Task<bool> CheckHealthAsync(int id);
}