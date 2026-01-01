using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IDomainsRepository
{
    Task<List<DomainEntity>> GetAllAsync(CancellationToken ct);
    Task<DomainEntity?> GetByIdAsync(Guid id, CancellationToken ct);
    Task CreateAsync(DomainEntity domainEntity, CancellationToken ct);
    void Delete(DomainEntity domainEntity);
    void Update(DomainEntity domainEntity);
}