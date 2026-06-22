namespace DomainScanner.Application.Abstractions.Persistence.Common;

public interface IWriteRepository<TEntity>
{
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
}