using DomainScanner.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

/// <summary>
/// Implements the Unit of Work pattern for managing database transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ScannerDbContext _context;
    private IDbContextTransaction? _transaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitOfWork"/> class. 
    /// </summary>
    /// <param name="context">The database context to be used for operations.</param>
    public UnitOfWork(ScannerDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct)
    { 
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken ct)
    {
        try
        {
            await SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
    }

   /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken ct)
    {
        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            _transaction?.Dispose();
            _transaction = null;
        }
    }

    /// <inheritdoc />
    public void Attach<T>(T entity) where T : class
    {
        _context.Attach(entity);
    }
}