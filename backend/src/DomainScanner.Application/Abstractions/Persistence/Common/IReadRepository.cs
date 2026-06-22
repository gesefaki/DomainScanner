using System.Linq.Expressions;

namespace DomainScanner.Application.Abstractions.Persistence.Common;

public interface IReadRepository<TEntity>
{
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct);
    Task<IEnumerable<TEntity>> GetAllWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    Task<IEnumerable<TEntity>> GetBatchAsync(int batchSize, CancellationToken ct);
    Task<TEntity?> FindAsync(Guid id, CancellationToken ct);
    Task<bool> IsExistsByAttribute(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
}