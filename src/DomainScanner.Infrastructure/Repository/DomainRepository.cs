using DomainScanner.Core.Models;
using DomainScanner.Core.Interfaces;
using DomainScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Repository;

public class DomainRepository(ScannerDbContext db) : IDomainRepository
{
    private readonly ScannerDbContext _db = db;

    public List<Domain> GetAll() => [.. _db.Domains.AsNoTracking()];

    public Domain? GetById(int id)
    {
        var domain = _db.Domains.FirstOrDefault(d => d.Id == id);
        return domain ?? null;
    }

    public bool IsExistsById(int id)
    {
        var domain = _db.Domains.AsNoTracking().FirstOrDefault(d => d.Id == id);
        return domain != null;
    }

    public void Add(Domain domain)
    {
        _db.Domains.Add(domain);
        _db.SaveChanges();
    }

    public void Update(Domain domain)
    {
        _db.Domains.Update(domain);
        _db.SaveChanges();
    }

    public void Remove(int id)
    {
        var domain = GetById(id);
        if (domain is null) return;
        _db.Domains.Remove(domain);
        _db.SaveChanges();
    }
}