using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// In-memory implementation of a <see cref="IReadRepository{DomainEntity, Guid}"/> used by integration tests.
/// </summary>
/// <param name="domains">
/// The collection of domain entities used as test data.
/// </param>
internal sealed class TestDomainRepository(IEnumerable<DomainEntity> domains)
    : IReadRepository<DomainEntity, Guid>
{
    
    private readonly IReadOnlyCollection<DomainEntity> _domains = domains.ToArray();
    
    /// <summary>
    /// Returns all <see cref="DomainEntity"/> that satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">
    /// The predicate used to filter domain entities.
    /// </param>
    /// <param name="ct">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// A collection of <see cref="DomainEntity"/> that satisfy the specified predicate.
    /// </returns>
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
