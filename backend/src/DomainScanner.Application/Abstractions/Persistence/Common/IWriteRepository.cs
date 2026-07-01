using DomainScanner.Domain.Common;

namespace DomainScanner.Application.Abstractions.Persistence.Common;

public interface IWriteRepository<TEntity, TId>
    where TEntity : BaseEntity
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
}