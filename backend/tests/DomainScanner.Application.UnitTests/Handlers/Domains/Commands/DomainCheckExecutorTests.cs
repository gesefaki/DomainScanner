using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Services.Domains;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="DomainCheckExecutor"/>.
/// </summary>
public class DomainCheckExecutorTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _domains = new();
    private readonly Mock<IWriteRepository<DomainCheckResult, Guid>> _checks = new();
    private readonly Mock<IHttpScanner> _http = new();
    private readonly DomainCheckExecutor _executor;

    public DomainCheckExecutorTests()
    {
        _executor = new DomainCheckExecutor(
            _domains.Object,
            _checks.Object,
            _http.Object);
    }

    /// <summary>
    /// A successful HTTP check is persisted, added to the domain history, and updates domain availability.
    /// </summary>
    [Fact]
    public async Task ExecuteAndSaveAsync_WhenCheckSucceeds_PersistsResultAndUpdatesDomain()
    {
        // Arrange
        var domain = new DomainBuilder()
            .WithId(Guid.NewGuid())
            .WithAddress("https://example.com/")
            .Inactive()
            .Build();
        var response = new HttpResponseObject
        {
            Address = domain.Address,
            StatusCode = 200,
            IsSuccess = true
        };
        DomainCheckResult? persistedCheck = null;

        _http
            .Setup(x => x.GetHttpResponseAsync(
                It.Is<Uri>(uri => uri.AbsoluteUri == domain.Address),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _checks
            .Setup(x => x.CreateAsync(
                It.IsAny<DomainCheckResult>(),
                It.IsAny<CancellationToken>()))
            .Callback<DomainCheckResult, CancellationToken>(
                (check, _) => persistedCheck = check)
            .ReturnsAsync((DomainCheckResult check, CancellationToken _) => check);
        _domains
            .Setup(x => x.Update(domain))
            .Returns(domain);

        // Act
        var result = await _executor.ExecuteAndSaveAsync(
            domain,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(persistedCheck);
        result.DomainId.Should().Be(domain.Id);
        result.Address.Should().Be(domain.Address);
        result.StatusCode.Should().Be(200);
        result.IsActive.Should().BeTrue();
        domain.IsActive.Should().BeTrue();
        domain.UpdatedAt.Should().NotBeNull();
        domain.CheckResults.Should().ContainSingle().Which.Should().BeSameAs(result);

        _checks.Verify(x => x.CreateAsync(
            result,
            It.IsAny<CancellationToken>()), Times.Once);
        _domains.Verify(x => x.Update(domain), Times.Once);
    }

    /// <summary>
    /// An invalid stored address is rejected before an HTTP request or persistence operation occurs.
    /// </summary>
    [Fact]
    public async Task ExecuteAndSaveAsync_WhenStoredAddressIsInvalid_ThrowsAndDoesNotPersist()
    {
        // Arrange
        var domain = new DomainBuilder()
            .WithAddress("not-a-valid-uri")
            .Build();

        // Act
        var action = () => _executor.ExecuteAndSaveAsync(
            domain,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DomainInvalidAddressFormatException>();
        _http.Verify(x => x.GetHttpResponseAsync(
            It.IsAny<Uri>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _checks.Verify(x => x.CreateAsync(
            It.IsAny<DomainCheckResult>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _domains.Verify(x => x.Update(
            It.IsAny<DomainEntity>()), Times.Never);
    }
}
