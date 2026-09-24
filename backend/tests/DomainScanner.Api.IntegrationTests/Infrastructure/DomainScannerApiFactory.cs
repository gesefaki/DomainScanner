extern alias DomainScannerApi;
using System.Net.Http.Headers;
using DomainScanner.Api.IntegrationTests.Controllers;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.Common;
using DomainScanner.Domain.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ApiProgram = DomainScannerApi::Program;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Provides a configured application factory for API integration tests.
/// </summary>
public sealed class DomainScannerApiFactory : WebApplicationFactory<ApiProgram>
{
    // Base64-encoded 32-byte value used only by the in-memory test host.
    private const string TestLoginAccountHmacSecret =
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    /// <summary>
    /// Identifier of the first predefined test user.
    /// </summary>
    public static readonly Guid UserAId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    /// <summary>
    /// Identifier of the second predefined test user.
    /// </summary>
    public static readonly Guid UserBId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    /// <summary>
    /// Configures the application host and replaces production dependencies
    /// with test-specific implementations.
    /// </summary>
    /// <param name="builder">
    /// The web host builder used to configure the test application.
    /// </param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .UseSetting(
                "ConnectionStrings:PostgresConnection",
                "Host=localhost;Port=1;Database=tests;Username=tests;Password=tests")
            .UseSetting(
                "ConnectionStrings:RedisConnection",
                "localhost:1,abortConnect=false")
            .UseSetting("JwtOptions:Issuer", "DomainScanner.IntegrationTests")
            .UseSetting("JwtOptions:Audience", "DomainScanner.Api")
            .UseSetting(
                "JwtOptions:SecretKey",
                "integration-tests-only-secret-key-32-bytes-minimum")
            .UseSetting("JwtOptions:ExpiresHours", "1")
            .UseSetting(
                "LoginAccountKeyOptions:HmacSecret",
                TestLoginAccountHmacSecret)
            .UseSetting("RedisCacheOptions:InstanceName", "integration-tests")
            .UseSetting("RedisCacheOptions:DefaultExpirationMinutes", "1");

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services
                .AddDataProtection()
                .UseEphemeralDataProtectionProvider();

            services
                .AddControllers()
                .AddApplicationPart(
                    typeof(RateLimitingProbeController).Assembly
                );

            services.RemoveAll<IHostedService>();

            services.AddSingleton<ScanConcurrencyProbe>();

            ReplaceRepository(
                services,
                new TestRepository<DomainEntity>(
                [
                    CreateDomain(
                        "aaaaaaaa-0000-0000-0000-000000000001",
                        "https://a-one.example/",
                        UserAId),
                    CreateDomain(
                        "aaaaaaaa-0000-0000-0000-000000000002",
                        "https://a-two.example/",
                        UserAId),
                    CreateDomain(
                        "bbbbbbbb-0000-0000-0000-000000000001",
                        "https://b-one.example/",
                        UserBId)
                ]));

            ReplaceRepository(
                services,
                new TestRepository<User>());

            ReplaceRepository(
                services,
                new TestRepository<DomainCheckResult>());

            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton<IUnitOfWork, TestUnitOfWork>();

            services.RemoveAll<IHttpScanner>();
            services.AddSingleton<IHttpScanner, TestHttpScanner>();
        });
    }

    /// <summary>
    /// Replaces all repository interfaces for an entity type with one shared
    /// in-memory repository instance.
    /// </summary>
    /// <typeparam name="TEntity">The entity type stored by the repository.</typeparam>
    /// <param name="services">The test host service collection.</param>
    /// <param name="repository">The repository instance used by the test host.</param>
    private static void ReplaceRepository<TEntity>(
        IServiceCollection services,
        TestRepository<TEntity> repository)
        where TEntity : BaseEntity
    {
        services.RemoveAll<IRepository<TEntity, Guid>>();
        services.RemoveAll<IReadRepository<TEntity, Guid>>();
        services.RemoveAll<IWriteRepository<TEntity, Guid>>();

        services.AddSingleton<IRepository<TEntity, Guid>>(repository);
        services.AddSingleton<IReadRepository<TEntity, Guid>>(repository);
        services.AddSingleton<IWriteRepository<TEntity, Guid>>(repository);
    }

    /// <summary>
    /// Generates an access token for the specified test user.
    /// </summary>
    /// <param name="userId">
    /// The identifier of the user for whom the access token is generated.
    /// </param>
    /// <returns>
    /// A JWT access token for the specified user.
    /// </returns>
    private string CreateAccessToken(Guid userId)
    {
        using var scope = Services.CreateScope();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        return jwtProvider.GenerateToken(new User
        {
            Id = userId
        });
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> authenticated as the specified test user.
    /// </summary>
    /// <param name="factory">
    /// The application factory used to create the HTTP client
    /// and generate the access token.
    /// </param>
    /// <param name="userId">
    /// The identifier of the user to authenticate as.
    /// </param>
    /// <returns>
    /// An HTTP client configured with a bearer access token.
    /// </returns>
    public HttpClient CreateAuthenticatedClient(
        DomainScannerApiFactory factory,
        Guid userId)
    {
        var client = factory.CreateHttpsClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", factory.CreateAccessToken(userId));

        return client;
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> that uses HTTPS as its base address.
    /// </summary>
    /// <returns>
    /// An HTTP client configured with <c>https://localhost</c>
    /// as its base address.
    /// </returns>
    public HttpClient CreateHttpsClient()
    {
        return CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });
    }

    /// <summary>
    /// Creates a <see cref="DomainEntity"/> for use in integration test data.
    /// </summary>
    /// <param name="id">
    /// The domain entity identifier.
    /// </param>
    /// <param name="address">
    /// The domain address.
    /// </param>
    /// <param name="userId">
    /// The identifier of the user who owns the domain.
    /// </param>
    /// <returns>
    /// A configured <see cref="DomainEntity"/>.
    /// </returns>
    private static DomainEntity CreateDomain(
        string id,
        string address,
        Guid userId) =>
        new()
        {
            Id = Guid.Parse(id),
            Address = address,
            UserId = userId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
}
