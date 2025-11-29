using DomainScanner.Core.Models;

namespace DomainScanner.Core.Interfaces;

public interface IDomainService
{
    public Task<IEnumerable<Domain>> GetAllAsync();
    public Task<Domain?> GetByIdAsync(int id);
    public Task<bool> IsExistsByIdAsync(int id);
    public Task AddAsync(Domain domain);
    public Task RemoveAsync(int id);
    public Task UpdateAsync(int id, Domain domain);
    public Task<bool> CheckHealthAsync(int id);
}