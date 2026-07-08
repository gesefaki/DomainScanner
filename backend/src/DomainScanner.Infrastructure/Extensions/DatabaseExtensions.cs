using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Infrastructure.Extensions;

/// <summary>
/// Applies extensions associated with the database used by the DbContext instance.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Utilises migrations, hiding the implementation details internally for ease of use within the composition root.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to which the migrations will apply.</param>
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