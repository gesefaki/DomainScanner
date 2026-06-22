using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IDomainCheckRepository : IReadRepository<DomainCheckResult>, IWriteRepository<DomainCheckResult>
{
}