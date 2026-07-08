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
    protected readonly DbSet<TEntity> DbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{TEntity, TId}"/> class. 
    /// </summary>
    /// <param name="context">The database context to be used for operations.</param>
    public Repository(ScannerDbContext context)
    {
        _context = context;
        DbSet = _context.Set<TEntity>();
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(int batchSize, CancellationToken ct)
    {
        return await DbSet
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
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FindAsync(TId id, CancellationToken ct)
    {
        return await DbSet.FindAsync([id], ct);
    }

    /// <inheritdoc />
    public virtual async Task<bool> IsExistsByAttribute(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await DbSet.AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var result = await DbSet.AddAsync(entity, ct);
        return result.Entity;
    }

    /// <inheritdoc />
    public virtual TEntity Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var result = DbSet.Update(entity);
        return result.Entity;
    }

    /// <inheritdoc />
    public virtual void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        DbSet.Remove(entity);
    }
}