using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context.Utils;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

public class ScannerDbContext : DbContext
{
    public DbSet<DomainEntity> Domains { get; set; } 
    public DbSet<DomainCheckResult> CheckResults { get; set; }
    public DbSet<User> Users { get; set; }
    
    public ScannerDbContext(DbContextOptions<ScannerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        CustomModelBuilder.OnModelCreating(builder);
        
        base.OnModelCreating(builder);
    }
}