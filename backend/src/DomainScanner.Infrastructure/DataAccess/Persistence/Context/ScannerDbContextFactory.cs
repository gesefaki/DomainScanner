using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

public class ScannerDbContextFactory : IDesignTimeDbContextFactory<ScannerDbContext>
{
    public ScannerDbContext CreateDbContext(string[] args)
    {
        var projectPath = Path.Combine(Directory.GetCurrentDirectory(), "..\\DomainScanner.Api");
        
        var config = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ScannerDbContext>();
        optionsBuilder.UseNpgsql(config.GetConnectionString("PostgresConnection"));
        
        return new ScannerDbContext(optionsBuilder.Options);
    }
}