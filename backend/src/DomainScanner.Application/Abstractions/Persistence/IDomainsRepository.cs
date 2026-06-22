using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IDomainsRepository : IReadRepository<DomainEntity>, IWriteRepository<DomainEntity>
{
}