using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DomainScanner.Infrastructure.Data;

public class ScannerDbContextFactory : IDesignTimeDbContextFactory<ScannerDbContext>
{
    public ScannerDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<ScannerDbContext>();
        
        builder.UseNpgsql("Host=localhost;Database=domainscanner;Username=postgres;Password=postgres");
        
        return new ScannerDbContext(builder.Options);
    }
}