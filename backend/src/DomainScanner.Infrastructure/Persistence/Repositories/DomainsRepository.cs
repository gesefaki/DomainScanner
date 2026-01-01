using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class DomainsRepository(ScannerDbContext context) : IDomainsRepository
{
    private readonly ScannerDbContext _context = context;

    public async Task<List<DomainEntity>> GetAllAsync(CancellationToken ct)
        => await _context.Domains.ToListAsync(ct);

    public async Task<DomainEntity?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Domains.FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task CreateAsync(DomainEntity domain, CancellationToken ct)
        => await _context.Domains.AddAsync(domain, ct);
    
    public void Delete(DomainEntity domain)
        => _context.Domains.Remove(domain);
    
    public void Update(DomainEntity domain)
        => _context.Domains.Update(domain);
}