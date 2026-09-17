using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="HttpSendAndSaveOwnedCommandHandler"/>.
/// </summary>
public class HttpSendAndSaveOwnedCommandHandlerTests
{
    private readonly Mock<IOwnedDomainProvider> _ownedDomains = new();
    private readonly Mock<IDomainCheckExecutor> _executor = new();
    private readonly HttpSendAndSaveOwnedCommandHandler _handler;

    public HttpSendAndSaveOwnedCommandHandlerTests()
    {
        _handler = new HttpSendAndSaveOwnedCommandHandler(
            _ownedDomains.Object,
            _executor.Object);
    }

    /// <summary>
    /// An owned domain is passed to the shared check executor.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainIsOwned_DelegatesToExecutor()
    {
        // Arrange
        var domainId = Guid.NewGuid();
        var command = new HttpSendAndSaveOwnedCommand(domainId);
        var domain = new DomainBuilder().WithId(domainId).Build();
        var expected = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            DomainId = domainId,
            Address = domain.Address
        };

        _ownedDomains
            .Setup(x => x.GetRequiredAsync(
                domainId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);
        _executor
            .Setup(x => x.ExecuteAndSaveAsync(
                domain,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expected);
        _executor.Verify(x => x.ExecuteAndSaveAsync(
            domain,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// A missing or foreign domain is rejected before the check executor is invoked.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainIsNotOwned_ThrowsAndDoesNotExecuteCheck()
    {
        // Arrange
        var domainId = Guid.NewGuid();
        var command = new HttpSendAndSaveOwnedCommand(domainId);

        _ownedDomains
            .Setup(x => x.GetRequiredAsync(
                domainId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException(domainId));

        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();
        _executor.Verify(x => x.ExecuteAndSaveAsync(
            It.IsAny<DomainEntity>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }
}
