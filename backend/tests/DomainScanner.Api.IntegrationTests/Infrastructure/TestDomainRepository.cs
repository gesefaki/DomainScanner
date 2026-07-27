using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

internal sealed class TestDomainRepository(IEnumerable<DomainEntity> domains)
    : IReadRepository<DomainEntity, Guid>
{
    private readonly IReadOnlyCollection<DomainEntity> _domains = domains.ToArray();

    public Task<IEnumerable<DomainEntity>> GetAllWhereAsync(
        Expression<Func<DomainEntity, bool>> predicate,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IEnumerable<DomainEntity> result = _domains
            .Where(predicate.Compile())
            .ToArray();

        return Task.FromResult(result);
    }

    public Task<DomainEntity?> GetAsync(
        Expression<Func<DomainEntity, bool>> predicate,
        CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<IEnumerable<DomainEntity>> GetAllAsync(CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<IEnumerable<DomainEntity>> GetBatchAsync(
        int batchSize,
        CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<DomainEntity?> FindAsync(Guid id, CancellationToken ct) =>
        throw new NotSupportedException();

    public Task<bool> IsExistsByAttribute(
        Expression<Func<DomainEntity, bool>> predicate,
        CancellationToken ct) =>
        throw new NotSupportedException();
}
