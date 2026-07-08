using DomainScanner.Domain.Common;

namespace DomainScanner.Application.Abstractions.Persistence.Common;

/// <summary>
/// Defines write operations for a repository.
/// </summary>
/// <typeparam name="TEntity">Type of entity this repository manages. Must inherit from <see cref="BaseEntity"/>.</typeparam>
/// <typeparam name="TId">Type of entity's primary key. Must be a value type (struct).</typeparam>
public interface IWriteRepository<TEntity, TId>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Creates a new entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to create.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that returns the created entity.</returns>
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct);

    /// <summary>
    /// Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity with updated value.</param>
    /// <returns>The updated entity.</returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// Deletes an entity from the repository.
    /// </summary>
    /// <param name="entity">The entity to delete.</param>
    void Delete(TEntity entity);
}