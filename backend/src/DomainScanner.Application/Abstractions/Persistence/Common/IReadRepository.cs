using System.Linq.Expressions;
using DomainScanner.Domain.Common;

namespace DomainScanner.Application.Abstractions.Persistence.Common;

/// <summary>
/// Defines read-only operations for a repository.
/// </summary>
/// <typeparam name="TEntity">The type of entity this repository manages. Must inherit from <see cref="BaseEntity"/>.</typeparam>
/// <typeparam name="TId">The type of entity's primary key. Must be a value type (struct).</typeparam>
public interface IReadRepository<TEntity, TId>
    where TEntity : BaseEntity
    where TId : struct
{
    /// <summary>
    /// Retrieves a single entity that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The expression to filter entities.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that returns first matching entity, or null if no entity matches predicate.</returns>
    Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);

    /// <summary>
    /// Retrieves all entities from database.
    /// </summary>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that return all entities.</returns>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves all entity identifiers ordered by creation date, then by identifier, in ascending order.
    /// </summary>
    /// <param name="ct">The cancellation token for the query.</param>
    /// <returns>
    /// A task whose result is a read-only list of <see cref="BaseEntity.Id"/> values,
    /// or an empty list when no entities exist.
    /// </returns>
    Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct);

    /// <summary>
    /// Retrieves all entities that match the specified predicate.
    /// </summary>
    /// <param name="predicate">The expression to filter entities.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that returns all matching entities.</returns>
    Task<IEnumerable<TEntity>> GetAllWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);

    /// <summary>
    /// Retrieves a batch of entities ordered by creation date.
    /// </summary>
    /// <param name="batchSize">The maximum number of entities to retrieve.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns>A task representing the async operation that returns the batch of entities.</returns>
    Task<IEnumerable<TEntity>> GetBatchAsync(int batchSize, CancellationToken ct);

    /// <summary>
    /// Finds an entity by its primary key.
    /// </summary>
    /// <param name="id">Primary key value of entity to find.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns></returns>
    Task<TEntity?> FindAsync(TId id, CancellationToken ct);

    /// <summary>
    /// Checks if any entity exists that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">The expression to filter entities.</param>
    /// <param name="ct">Cancellation token provided by the user.</param>
    /// <returns></returns>
    Task<bool> IsExistsByAttribute(Expression<Func<TEntity, bool>> predicate, CancellationToken ct);
    
    /// <summary>Counts entities matching the predicate without loading them.</summary>
    /// <param name="predicate">The filter, including ownership constraints when required.</param>
    /// <param name="ct">The cancellation token for the query.</param>
    /// <returns>The number of matching entities.</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct);
}
