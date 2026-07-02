using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ScannerDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScannerDbContext>>();

        try
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {count} migrations...", pendingMigrations.Count());
                await context.Database.MigrateAsync();
            }
            else
            {
                logger.LogInformation("No migrations for applying.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while migrating the database");
            throw;
        }
    }
}