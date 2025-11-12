using DomainScanner.DataAccess.Postgre.Models;

namespace DomainScanner.DataAccess.Postgre.Repository;

public interface IDomainRepository
{
    public List<Domain> GetAll();
    public Domain? Get(int id);
    public void Add(Domain domain);
    public void Update(Domain domain);
    public void Remove(int id);

}