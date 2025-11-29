using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainRepository
{
    public List<Domain> GetAll();
    public Domain? GetById(int id);
    public bool IsExistsById(int id);
    public void Add(Domain domain);
    public void Update(Domain domain);
    public void Remove(int id);

}