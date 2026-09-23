using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Common;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;

/// <summary>
/// Generic repository implementation providing CRUD operations for entities. 
/// Default implementation of <see cref="IRepository{TEntity, TId}"/> 
/// </summary>
/// <typeparam name="TEntity">Type of entity this repository manages. Must inherit from <see cref="BaseEntity"/></typeparam>
/// <typeparam name="TId">Type of entity's primary key. Must be a value type (struct).</typeparam>
public class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : BaseEntity
    where TId : struct
{
    private readonly ScannerDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TId}"/> class. 
    /// </summary>
    /// <param name="context">The database context to be used for operations.</param>
    public Repository(ScannerDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await _dbSet.FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _dbSet.AsNoTracking().ToListAsync(ct);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Projects only entity identifiers without materializing or tracking entities.
    /// Automatically included navigation properties are excluded from the query.
    /// </remarks>
    public virtual async Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct)
    {
        return await _dbSet
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Select(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(int batchSize, CancellationToken ct)
    {
        return await _dbSet
            .IgnoreAutoIncludes()
            .OrderBy(entity => entity.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await _dbSet.Where(predicate).ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FindAsync(TId id, CancellationToken ct)
    {
        return await _dbSet.FindAsync([id], ct);
    }

    /// <inheritdoc />
    public virtual async Task<bool> IsExistsByAttribute(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await _dbSet.AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var result = await _dbSet.AddAsync(entity, ct);
        return result.Entity;
    }

    /// <inheritdoc />
    public virtual TEntity Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var result = _dbSet.Update(entity);
        return result.Entity;
    }

    /// <inheritdoc />
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        _dbSet.Remove(entity);
    }

    /// <inheritdoc />
    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct)
    {
        return await _dbSet
            .IgnoreAutoIncludes()
            .CountAsync(predicate, ct);
    }
}
