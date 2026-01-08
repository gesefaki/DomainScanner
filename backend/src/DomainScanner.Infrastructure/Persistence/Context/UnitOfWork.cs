using DomainScanner.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace DomainScanner.Infrastructure.Persistence.Context;

public class UnitOfWork : IUnitOfWork
{
    private readonly ScannerDbContext _context;

    public UnitOfWork(ScannerDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return _context.SaveChangesAsync(ct);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        return await _context.Database.BeginTransactionAsync(ct);
    }

    public void Attach<T>(T entity) where T : class
    {
        _context.Attach(entity);
    }
}