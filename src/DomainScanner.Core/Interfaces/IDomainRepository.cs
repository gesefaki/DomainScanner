using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainRepository
{
    public Task<List<Domain>> GetAllAsync();
    public Task<Domain?> GetByIdAsync(int id);
    public Task<bool> IsExistsByIdAsync(int id);
    public Task AddAsync(Domain domain);
    public Task UpdateAsync(Domain domain);
    public Task RemoveAsync(int id);

}