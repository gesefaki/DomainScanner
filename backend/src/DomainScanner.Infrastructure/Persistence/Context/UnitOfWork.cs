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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        return await _context.Database.BeginTransactionAsync(cancellationToken);
    }
}