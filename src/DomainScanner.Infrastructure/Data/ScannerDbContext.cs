using DomainScanner.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Data;

public class ScannerDbContext : DbContext
{
    public ScannerDbContext(DbContextOptions<ScannerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain> Domains { get; set; } = null!;
}