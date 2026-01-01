using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomainScanner.Infrastructure.Persistence.Context;

public class ScannerDbContextFactory : IDesignTimeDbContextFactory<ScannerDbContext>
{
    public ScannerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ScannerDbContext>();
        
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=DomainScanner;username=postgres;password=postgres");
        
        return new ScannerDbContext(optionsBuilder.Options);
    }
}