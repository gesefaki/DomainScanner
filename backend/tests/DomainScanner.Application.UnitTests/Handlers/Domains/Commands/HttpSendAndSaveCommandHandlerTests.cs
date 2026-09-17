using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="HttpSendAndSaveCommandHandler"/>.
/// </summary>
public class HttpSendAndSaveCommandHandlerTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _domainsRepository = new();
    private readonly Mock<IDomainCheckExecutor> _executor = new();
    private readonly HttpSendAndSaveCommandHandler _handler;

    private readonly Guid _domainId = Guid.NewGuid();

    public HttpSendAndSaveCommandHandlerTests()
    {
        _handler = new HttpSendAndSaveCommandHandler(
            _domainsRepository.Object,
            _executor.Object);
    }

    /// <summary>
    /// An existing domain is passed to the shared check executor and its result is returned.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainExists_DelegatesToExecutorAndReturnsResult()
    {
        // Arrange
        var command = new HttpSendAndSaveCommand(_domainId);
        var domain = new DomainBuilder()
            .WithId(_domainId)
            .Build();
        var expected = new DomainCheckResult
        {
            Id = Guid.NewGuid(),
            DomainId = _domainId,
            Address = domain.Address,
            StatusCode = 200,
            IsActive = true
        };

        _domainsRepository.SetupFindAsync(_domainId, domain);
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
    /// A missing domain produces a not-found error without invoking the check executor.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainDoesNotExist_ThrowsAndDoesNotExecuteCheck()
    {
        // Arrange
        var command = new HttpSendAndSaveCommand(_domainId);
        _domainsRepository.SetupFindAsync(_domainId, (DomainEntity?)null);

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
