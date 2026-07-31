using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Cache;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Options;
using DomainScanner.Contracts.Options.Auth;
using DomainScanner.Contracts.Options.Login;
using DomainScanner.Infrastructure.Auth.Authentication;
using DomainScanner.Infrastructure.Auth.Authentication.LoginProtection;
using DomainScanner.Infrastructure.Auth.Authentication.Normalization;
using DomainScanner.Infrastructure.Auth.Hashing;
using DomainScanner.Infrastructure.DataAccess.Cache;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using DomainScanner.Infrastructure.DataAccess.Persistence.Repositories;
using DomainScanner.Infrastructure.Extensions;
using DomainScanner.Infrastructure.Protocols.HTTP;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DomainScanner.Infrastructure.DI;

/// <summary>
/// Provides extension methods for configuring DI in the infrastructure layer. 
/// Centralizes all infrastructure service registrations, making it easy to configure the application's infrastructure dependencies from the composition root.
/// </summary>
public static class DependencyInjection
{

    /// <summary>
    /// Registers core infrastructure services including repositories, unit of work, auth services and HTTP scanning providers.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration"> The app configuration containing settings for infrastructure services.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddLoginProtection(configuration);
        
        // Add HTTP client and related services.
        services.AddHttpExtensions();

        // Register generic repository
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IReadRepository<,>), typeof(Repository<,>));
        services.AddScoped(typeof(IWriteRepository<,>), typeof(Repository<,>));

        // Register UOW
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register HTTP services
        services.AddScoped<IHttpScanner, HttpService>();

        // Register auth services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        return services;
    }
    
    /// <summary>
    /// Registers PostgreSQL database context with EF Core.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
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

    /// <summary>
    /// Registers Redis distributed caching services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The application configuration containing the connection string.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.ConfigureRedisLoginProtection(configuration);
        
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            return ConnectionMultiplexer.Connect(
                configuration.GetConnectionString("RedisConnection")!);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        services.AddScoped(typeof(ICacheKeyGenerator<>), typeof(CacheKeyGenerator<>));

        return services;
    }

    private static IServiceCollection ConfigureRedisLoginProtection(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<LoginProtectionOptions>()
            .Bind(configuration.GetSection(
                nameof(LoginProtectionOptions)))
            .Validate(
                options => options.LockoutThreshold > 0,
                "LockoutThreshold must be greater than zero.")
            .Validate(
                options => options.FailureWindowMinutes > 0,
                "FailureWindowMinutes must be greater than zero.")
            .Validate(
                options => options.LockoutDurationMinutes > 0,
                "LockoutDurationMinutes must be greater than zero.")
            .Validate(
                options =>
                    options.MaximumLockoutMinutes >=
                    options.LockoutDurationMinutes,
                "Maximum lockout must be at least the initial lockout.")
            .Validate(
                options =>
                    options.DelayStartAttempt > 0 &&
                    options.DelayStartAttempt <
                    options.LockoutThreshold,
                "DelayStartAttempt must be below LockoutThreshold.")
            .ValidateOnStart();

        services.AddSingleton<ILoginAttemptProtector, RedisLoginAttemptProtector>();

        return services;
    }

    private static IServiceCollection AddLoginProtection(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IEmailNormalizer, EmailNormalizer>();

        services.AddOptions<LoginAccountKeyOptions>()
            .Bind(configuration.GetSection(
                nameof(LoginAccountKeyOptions)))
            .Validate(
                options => !string.IsNullOrWhiteSpace(
                    options.HmacSecret),
                "Login account HMAC secret is required.")
            .ValidateOnStart();

        services.AddSingleton<ILoginAccountKeyProvider, HmacLoginAccountKeyProvider>();

        return services;
    }
}