using DomainScanner.DataAccess.Postgre.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.DataAccess.Postgre.Data;

public class ScannerDbContext : DbContext
{
    public ScannerDbContext(DbContextOptions<ScannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain> Domains { get; set; } = null!;
}