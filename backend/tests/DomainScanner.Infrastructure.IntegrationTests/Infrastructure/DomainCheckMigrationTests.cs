using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.DataAccess.Persistence.Context;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace DomainScanner.Infrastructure.IntegrationTests.Infrastructure;

/// <summary>Checks migration of existing HTTP history and round-tripping new outcomes.</summary>
public sealed class DomainCheckMigrationTests
{
    [Fact]
    public async Task ExistingChecksAreBackfilledAndTransportErrorsCanBeStored()
    {
        await using var container = new ContainerBuilder("postgres:17-alpine")
            .WithEnvironment("POSTGRES_USER", "audit")
            .WithEnvironment("POSTGRES_PASSWORD", "audit")
            .WithEnvironment("POSTGRES_DB", "domainscanner")
            .WithPortBinding(5432, true)
            .Build();
        await container.StartAsync();

        var connection =
            $"Host=127.0.0.1;Port={container.GetMappedPublicPort(5432)};" +
            "Database=domainscanner;Username=audit;Password=audit";
        await WaitForPostgresAsync(connection);
        var options = new DbContextOptionsBuilder<ScannerDbContext>()
            .UseNpgsql(connection)
            .Options;
        await using var db = new ScannerDbContext(options);
        var migrations = db.GetService<IMigrator>();
        await migrations.MigrateAsync(
            "20260923142432_AddMonitoringAndRetentionIndexes");

        var userId = Guid.NewGuid();
        var domainId = Guid.NewGuid();
        var oldCheckId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO "Users"
                 ("Id", "Username", "Email", "NormalizedEmail",
                  "PasswordHash", "IsActive", "CreatedAt")
             VALUES
                 ({userId}, 'audit', 'audit@example.com',
                  'audit@example.com', 'hash', TRUE, {now});
             """);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO "Domains"
                 ("Id", "Address", "UserId", "MonitoringEnabled",
                  "IsActive", "CreatedAt")
             VALUES
                 ({domainId}, 'https://example.com/', {userId},
                  TRUE, FALSE, {now});
             """);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"""
             INSERT INTO "CheckResults"
                 ("Id", "Address", "StatusCode", "DomainId",
                  "IsActive", "CreatedAt")
             VALUES
                 ({oldCheckId}, 'https://example.com/', 503,
                  {domainId}, FALSE, {now});
             """);

        await migrations.MigrateAsync();
        var oldCheck = await db.CheckResults
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .SingleAsync(check => check.Id == oldCheckId);
        Assert.Equal("http", oldCheck.Kind);
        Assert.Equal("down", oldCheck.Outcome);
        Assert.Equal("https://example.com/", oldCheck.RequestedAddress);
        Assert.Equal("https://example.com/", oldCheck.FinalAddress);
        Assert.Equal(503, oldCheck.StatusCode);

        var failure = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            DomainId = domainId,
            Kind = "http",
            Outcome = "error",
            RequestedAddress = "https://example.com/",
            FinalAddress = null,
            StatusCode = null,
            ErrorCode = "dns_error",
            ResponseTimeMs = 15,
            Redirects = ["https://www.example.com/"],
            TlsHasValidationErrors = true,
            TlsCertificateExpiresAt = now.AddDays(30),
            CreatedAt = now.AddSeconds(1)
        };
        db.CheckResults.Add(failure);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var persisted = await db.CheckResults
            .IgnoreAutoIncludes()
            .AsNoTracking()
            .SingleAsync(check => check.Id == failure.Id);
        Assert.Null(persisted.StatusCode);
        Assert.Null(persisted.FinalAddress);
        Assert.Equal("dns_error", persisted.ErrorCode);
        Assert.Equal(15, persisted.ResponseTimeMs);
        Assert.Equal(["https://www.example.com/"], persisted.Redirects);
        Assert.True(persisted.TlsHasValidationErrors);
        Assert.True(
            (now.AddDays(30) - persisted.TlsCertificateExpiresAt!.Value).Duration()
            < TimeSpan.FromMicroseconds(1));
    }

    private static async Task WaitForPostgresAsync(string connection)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                await using var probe = new NpgsqlConnection(connection);
                await probe.OpenAsync();
                return;
            }
            catch (NpgsqlException) when (attempt < 29)
            {
                await Task.Delay(500);
            }
        }
    }
}
