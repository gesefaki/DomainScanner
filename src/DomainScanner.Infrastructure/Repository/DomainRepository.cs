using DomainScanner.Infrastructure.Models;
using DomainScanner.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Repository;

public class DomainRepository(ScannerDbContext db) : IDomainRepository
{
    private readonly ScannerDbContext _db = db;

    public List<Domain> GetAll() => [.. _db.Domains.AsNoTracking()];

    public Domain? Get(int id)
    {
        var domain = _db.Domains.FirstOrDefault(d => d.Id == id);
        if (domain is not null)
            return domain;

        return null;
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
        var domain = Get(id);
        if (domain is not null)
        {
            _db.Domains.Remove(domain);
            _db.SaveChanges();
        }
    }
}