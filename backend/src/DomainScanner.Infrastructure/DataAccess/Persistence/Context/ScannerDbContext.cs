using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context.Utils;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

/// <summary>
/// EF Core database context for the Domain Scanner application.
/// </summary>
public class ScannerDbContext : DbContext
{
    /// <summary>
    /// Database Set for <see cref="DomainEntity"/> 
    /// </summary>
    public DbSet<DomainEntity> Domains { get; set; }

    /// <summary>
    /// Database Set for <see cref="DomainCheckResult"/>
    /// </summary>  
    public DbSet<DomainCheckResult> CheckResults { get; set; }

    /// <summary>
    /// Database Set for <see cref="User"/> 
    /// </summary>
    public DbSet<User> Users { get; set; }
    

    public ScannerDbContext(DbContextOptions<ScannerDbContext> options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        CustomModelBuilder.OnModelCreating(builder);
        base.OnModelCreating(builder);
    }
}