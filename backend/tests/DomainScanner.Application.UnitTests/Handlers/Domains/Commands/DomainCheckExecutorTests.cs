using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Services.Domains;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>Checks how scanner outcomes are persisted and reflected on a domain.</summary>
public sealed class DomainCheckExecutorTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _domains = new();
    private readonly Mock<IWriteRepository<DomainCheckResult, Guid>> _checks = new();
    private readonly Mock<IHttpScanner> _http = new();

    [Fact]
    public async Task HttpResponse_PersistsRemoteStatusAndOutcome()
    {
        var domain = new DomainBuilder().WithAddress("https://example.com/").Build();
        _http.Setup(scanner => scanner.CheckAsync(
                It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpScanResult(
                domain.Address, "https://www.example.com/", 503, 42,
                null, ["https://www.example.com/"],
                new TlsFetch
                {
                    SslPolicyErrors = false,
                    CertificateExpiresAt = new DateTime(
                        2027, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }));
        _checks.Setup(repository => repository.CreateAsync(
                It.IsAny<DomainCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainCheckResult result, CancellationToken _) => result);
        _domains.Setup(repository => repository.Update(domain)).Returns(domain);

        var result = await new DomainCheckExecutor(
            _domains.Object, _checks.Object, _http.Object)
            .ExecuteAndSaveAsync(domain, CancellationToken.None);

        Assert.Equal("http", result.Kind);
        Assert.Equal("down", result.Outcome);
        Assert.Equal(503, result.StatusCode);
        Assert.Null(result.ErrorCode);
        Assert.Equal(domain.Address, result.RequestedAddress);
        Assert.Equal("https://www.example.com/", result.FinalAddress);
        Assert.Equal(42, result.ResponseTimeMs);
        Assert.Single(result.Redirects);
        Assert.False(result.TlsHasValidationErrors);
        Assert.False(domain.IsActive);
        Assert.Contains(result, domain.CheckResults);
    }

    [Fact]
    public async Task TransportFailure_HasNoHttpStatus()
    {
        var domain = new DomainBuilder().WithAddress("https://example.com/").Build();
        _http.Setup(scanner => scanner.CheckAsync(
                It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpScanResult(
                domain.Address, null, null, 10000,
                "dns_error", [], null));
        _checks.Setup(repository => repository.CreateAsync(
                It.IsAny<DomainCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainCheckResult result, CancellationToken _) => result);
        _domains.Setup(repository => repository.Update(domain)).Returns(domain);

        var result = await new DomainCheckExecutor(
            _domains.Object, _checks.Object, _http.Object)
            .ExecuteAndSaveAsync(domain, CancellationToken.None);

        Assert.Equal("error", result.Outcome);
        Assert.Null(result.StatusCode);
        Assert.Null(result.FinalAddress);
        Assert.Equal("dns_error", result.ErrorCode);
        Assert.False(domain.IsActive);
    }

    [Fact]
    public async Task InvalidStoredAddress_DoesNotCallScannerOrPersistence()
    {
        var domain = new DomainBuilder().WithAddress("not-a-valid-uri").Build();
        await Assert.ThrowsAsync<DomainInvalidAddressFormatException>(() =>
            new DomainCheckExecutor(_domains.Object, _checks.Object, _http.Object)
                .ExecuteAndSaveAsync(domain, CancellationToken.None));
        _http.VerifyNoOtherCalls();
        _checks.VerifyNoOtherCalls();
    }
}
