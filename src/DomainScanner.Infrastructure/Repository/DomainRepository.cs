using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;
using DomainScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Repository;

public class DomainRepository(ScannerDbContext db) : IDomainRepository
{
    private readonly ScannerDbContext _db = db;

    public async Task<List<Domain>> GetAllAsync() => await _db.Domains.AsNoTracking().ToListAsync();

    public async Task<Domain?> GetByIdAsync(int id)
    {
        var domain = await _db.Domains.FirstOrDefaultAsync(d => d.Id == id);
        return domain ?? null;
    }

    public async Task<bool> IsExistsByIdAsync(int id)
    {
        var domain = await _db.Domains.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);
        return domain != null;
    }

    public async Task AddAsync(Domain domain)
    {
        await _db.Domains.AddAsync(domain);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Domain domain)
    {
        _db.Domains.Update(domain);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(int id)
    {
        var domain = await GetByIdAsync(id);
        
        if (domain is null) return;
        _db.Domains.Remove(domain);
        
        await _db.SaveChangesAsync();
    }
}