using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Common;

namespace DomainScanner.Application.Abstractions.Persistence;

/// <summary>
/// Combines read and write repository operations into a single interface.
/// Provides full CRUD.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages. Must inherit from <see cref="BaseEntity"/>.</typeparam>
/// <typeparam name="TId">The type of the entity's primary key. Must be a value type (struct).</typeparam>
public interface IRepository<TEntity, TId> 
    : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId> 
    where TEntity : BaseEntity
    where TId : struct;