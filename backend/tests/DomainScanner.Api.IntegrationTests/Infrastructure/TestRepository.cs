using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Common;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides an in-memory repository for API integration tests.
/// </summary>
/// <typeparam name="TEntity">The entity type stored by the repository.</typeparam>
internal sealed class TestRepository<TEntity> : IRepository<TEntity, Guid>
    where TEntity : BaseEntity
{
    private readonly object _sync = new();
    private readonly List<TEntity> _entities;

    /// <summary>
    /// Initializes a repository with optional seed data.
    /// </summary>
    /// <param name="entities">Entities available when the test host starts.</param>
    public TestRepository(IEnumerable<TEntity>? entities = null)
    {
        _entities = entities?.ToList() ?? [];
    }

    /// <inheritdoc />
    public Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(
                _entities.FirstOrDefault(predicate.Compile()));
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IEnumerable<TEntity>>(
                _entities.ToArray());
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Guid>>(
                _entities
                    .OrderBy(entity => entity.CreatedAt)
                    .ThenBy(entity => entity.Id)
                    .Select(entity => entity.Id)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetAllWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IEnumerable<TEntity>>(
                _entities.Where(predicate.Compile()).ToArray());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<TEntity>> GetBatchAsync(
        int batchSize,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult<IEnumerable<TEntity>>(
                _entities
                    .OrderBy(entity => entity.CreatedAt)
                    .Take(batchSize)
                    .ToArray());
        }
    }

    /// <inheritdoc />
    public Task<TEntity?> FindAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(
                _entities.FirstOrDefault(entity => entity.Id == id));
        }
    }

    /// <inheritdoc />
    public Task<bool> IsExistsByAttribute(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            return Task.FromResult(
                _entities.Any(predicate.Compile()));
        }
    }

    /// <inheritdoc />
    public Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        lock (_sync)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = Guid.NewGuid();
            }

            if (entity.CreatedAt == default)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }

            _entities.Add(entity);
            return Task.FromResult(entity);
        }
    }

    /// <inheritdoc />
    public TEntity Update(TEntity entity)
    {
        lock (_sync)
        {
            var index = _entities.FindIndex(current => current.Id == entity.Id);

            if (index >= 0)
            {
                _entities[index] = entity;
            }

            return entity;
        }
    }

    /// <inheritdoc />
    public void Delete(TEntity entity)
    {
        lock (_sync)
        {
            _entities.RemoveAll(current => current.Id == entity.Id);
        }
    }
}
