namespace DomainScanner.Application.Abstractions.Persistence;

/// <summary>
/// Defines the contract for Unit of Work pattern implementation.
/// </summary>
public interface IUnitOfWork
{
     /// <summary>
    /// Saves all pending changes to the database asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token provided by the user</param>>
    Task<int> SaveChangesAsync(CancellationToken ct);

    /// <summary>
    /// Begins a new database transaction asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token provided by the user.</param>
    Task BeginTransactionAsync(CancellationToken ct);

    /// <summary>
    /// Commits all pending changes and the current transaction asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token provided by the user.</param>
    Task CommitTransactionAsync(CancellationToken ct);

    /// <summary>
    /// Rolls back the current transaction asynchronously.
    /// </summary>
    /// <param name="ct">Cancellation token provided by the user.</param>
    Task RollbackTransactionAsync(CancellationToken ct);

    /// <summary>
    /// Attaches an entity to the context for tracking.
    /// </summary>
    /// <typeparam name="T">The type of entity being attached.</typeparam>
    /// <param name="entity">The entity to attach to the context.</param>
    void Attach<T>(T entity) where T : class;
}