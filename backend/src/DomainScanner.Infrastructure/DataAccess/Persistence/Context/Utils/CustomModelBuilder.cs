using DomainScanner.Infrastructure.DataAccess.Persistence.Context.Configuration;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.DataAccess.Persistence.Context.Utils;

internal static class CustomModelBuilder
{
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