using DomainScanner.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

public class UnitOfWork : IUnitOfWork
{
    private readonly ScannerDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ScannerDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct)
    { 
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

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

    public void Attach<T>(T entity) where T : class
    {
        _context.Attach(entity);
    }
}