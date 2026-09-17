using DomainScanner.Application.Abstractions.Persistence;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a no-op unit of work for in-memory API integration tests.
/// </summary>
internal sealed class TestUnitOfWork : IUnitOfWork
{
    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(1);
    }

    /// <inheritdoc />
    public Task BeginTransactionAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CommitTransactionAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RollbackTransactionAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Attach<T>(T entity) where T : class
    {
    }
}
