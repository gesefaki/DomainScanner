using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class DomainsRepository(ScannerDbContext context) : IDomainsRepository
{
    private readonly ScannerDbContext _context = context;

    public async Task<List<DomainEntity>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Domains
            .Include(d => d.User)
            .Include(d => d.CheckResults)
            .AsSplitQuery()
            .ToListAsync(ct);
    }
    

    public async Task<List<DomainEntity>> GetAllWithResultsAsync(CancellationToken ct)
    {
        return await _context.Domains
            .Include(d => d.CheckResults)
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    public async Task<DomainEntity?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Domains
            .Include(d => d.User)
            .Include(d => d.CheckResults)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }
    

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
    {
        return _context.Domains.AnyAsync(ct);
    }

    public async Task CreateAsync(DomainEntity domain, CancellationToken ct)
        => await _context.Domains.AddAsync(domain, ct);
    
    public void Delete(DomainEntity domain)
        => _context.Domains.Remove(domain);
    
    public void Update(DomainEntity domain)
        => _context.Domains.Update(domain);
}