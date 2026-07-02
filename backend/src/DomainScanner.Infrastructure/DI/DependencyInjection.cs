using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Infrastructure.Auth.Authentication;
using DomainScanner.Infrastructure.Auth.Hashing;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;
using DomainScanner.Infrastructure.Extensions;
using DomainScanner.Infrastructure.Protocols.HTTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DomainScanner.Infrastructure.DI;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpExtensions();

        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IReadRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(Repository<,>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IHttpScanner, HttpService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        return services;
    }
    
    public static IServiceCollection AddPostgresDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddDbContext<ScannerDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection"),
            x => x.MigrationsAssembly("DomainScanner.Infrastructure")
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        ));

        return services;
    }
}