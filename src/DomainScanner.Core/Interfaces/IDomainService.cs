using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainService
{
    public IEnumerable<Domain> GetAll();
    public Domain? GetById(int id);
    public void Add(Domain domain);
    public void Remove(int id);
    public void Update(Domain domain);
}