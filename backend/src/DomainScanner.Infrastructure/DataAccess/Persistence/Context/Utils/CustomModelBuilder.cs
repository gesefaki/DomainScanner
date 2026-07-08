using DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Utils;

/// <summary>
/// Centralizes model-building for <see cref="ScannerDbContext"/>
/// </summary>
internal static class CustomModelBuilder
{
    /// <summary>
    /// Applies all entity type configurations and shared model conventions.
    /// </summary>
    /// <param name="builder">The model builder to configure.</param>
    public static void OnModelCreating(ModelBuilder builder)
    {
        builder
            .ApplyConfiguration(new DomainEntityConfiguration())
            .ApplyConfiguration(new DomainCheckResultConfiguration())
            .ApplyConfiguration(new IpConfiguration())
            .ApplyConfiguration(new UserConfiguration());

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                navigation.SetIsEagerLoaded(true);
            }
        }
    }
}