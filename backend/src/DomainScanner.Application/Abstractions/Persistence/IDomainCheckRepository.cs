using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IDomainCheckRepository
{
    Task Create(DomainCheckResult check, CancellationToken cancellationToken);
}