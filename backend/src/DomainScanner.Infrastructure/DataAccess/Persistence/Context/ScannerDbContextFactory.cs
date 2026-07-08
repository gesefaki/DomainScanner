using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context;

/// <summary>
/// Factory class for creating <see cref="ScannerDbContext"/> instance at design-time. 
/// </summary>
public class ScannerDbContextFactory : IDesignTimeDbContextFactory<ScannerDbContext>
{
    /// <summary>
    /// Creates a new instance of <see cref="ScannerDbContext"/> for design-time operations. 
    /// </summary>
    /// <param name="args">Command-line arguments passed to the EF core tool.</param>
    /// <returns><see cref="ScannerDbContext"/> instance with Postgres connection string from API layer.</returns>
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