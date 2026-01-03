using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class DomainCheckRepository(ScannerDbContext context) : IDomainCheckRepository
{
    private readonly ScannerDbContext _context = context;
    
    public async Task Create(DomainCheckResult check, CancellationToken cancellationToken) =>
        await _context.CheckResults.AddAsync(check, cancellationToken);
}