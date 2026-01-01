using DomainScanner.Application.Abstractions.Persistence;

namespace DomainScanner.Infrastructure.Persistence.Context;

public class UnitOfWork : IUnitOfWork
{
    private readonly ScannerDbContext _context;

    public UnitOfWork(ScannerDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}