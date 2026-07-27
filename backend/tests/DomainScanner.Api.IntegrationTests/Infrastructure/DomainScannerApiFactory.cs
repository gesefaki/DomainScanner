extern alias DomainScannerApi;

using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ApiProgram = DomainScannerApi::Program;

namespace DomainScanner.Api.IntegrationTests.Infrastructure;

public sealed class DomainScannerApiFactory : WebApplicationFactory<ApiProgram>
{
    public static readonly Guid UserAId =
        Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static readonly Guid UserBId =
        Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

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
            .UseSetting("RedisCacheOptions:InstanceName", "integration-tests")
            .UseSetting("RedisCacheOptions:DefaultExpirationMinutes", "1");

        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<IReadRepository<DomainEntity, Guid>>();

            services.AddSingleton<IReadRepository<DomainEntity, Guid>>(
                new TestDomainRepository(
                [
                    CreateDomain(
                        "aaaaaaaa-0000-0000-0000-000000000001",
                        "a-one.example",
                        UserAId),
                    CreateDomain(
                        "aaaaaaaa-0000-0000-0000-000000000002",
                        "a-two.example",
                        UserAId),
                    CreateDomain(
                        "bbbbbbbb-0000-0000-0000-000000000001",
                        "b-one.example",
                        UserBId)
                ]));
        });
    }

    public string CreateAccessToken(Guid userId)
    {
        using var scope = Services.CreateScope();
        var jwtProvider = scope.ServiceProvider.GetRequiredService<IJwtProvider>();

        return jwtProvider.GenerateToken(new User
        {
            Id = userId
        });
    }

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
