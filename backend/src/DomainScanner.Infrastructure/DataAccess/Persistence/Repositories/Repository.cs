using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Common;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;

public class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : BaseEntity
    where TId : struct
{
private readonly ScannerDbContext _context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(ScannerDbContext context)
    {
        _context = context;
        DbSet = _context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetAsync(
        Expression<Func<TEntity,
        bool>> predicate,
        CancellationToken ct)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken ct)
    {
        return await DbSet.AsNoTracking().ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<TEntity>> GetBatchAsync(int batchSize, CancellationToken ct)
    {
        return await DbSet
            .IgnoreAutoIncludes()
            .OrderBy(entity => entity.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await DbSet.Where(predicate).ToListAsync(ct);
    }

    public virtual async Task<TEntity?> FindAsync(TId id, CancellationToken ct)
    {
        return await DbSet.FindAsync([id], ct);
    }

    public virtual async Task<bool> IsExistsByAttribute(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken ct)
    {
        return await DbSet.AnyAsync(predicate, ct);
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken ct)
    {
        var result = await DbSet.AddAsync(entity, ct);
        return result.Entity;
    }

    public virtual TEntity Update(TEntity entity)
    {
        var result = DbSet.Update(entity);
        return result.Entity;
    }

    public virtual void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}