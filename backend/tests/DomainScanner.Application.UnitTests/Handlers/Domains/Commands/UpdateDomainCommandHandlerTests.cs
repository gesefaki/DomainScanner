using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateDomainCommandHandler"/>.
/// </summary>
public class UpdateDomainCommandHandlerTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IOwnedDomainProvider> _ownedDomains = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateDomainCommandHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public UpdateDomainCommandHandlerTests()
    {
        _handler = new UpdateDomainCommandHandler(
            _ownedDomains.Object,
            _repository.Object,
            _mapper.Object);
    }

    /// <summary>
    /// Changing monitoring updates the owned domain without overwriting measured availability.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Handle_WhenDomainExists_UpdatesMonitoringWithoutChangingAvailability(bool enabled)
    {
        // Arrange
        var domain = new DomainBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Inactive()
            .Build();
        domain.MonitoringEnabled = !enabled;
        domain.IsActive = !enabled;
        
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .EnableMonitoring()
            .BuildUpdateCommand();
        command = command with { Request = command.Request with { MonitoringEnabled = enabled } };

        var expected = new DomainResponseBuilder().Build(domain);

        _ownedDomains
            .Setup(x => x.GetRequiredAsync(
                _fakeDomainId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(domain);

        _repository
            .Setup(x => x.Update(domain))
            .Returns(domain);

        _mapper
            .Setup(x => x.Map<DomainResponse>(domain))
            .Returns(expected);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        domain.Address.Should().Be(FakeDomainAddress);
        domain.MonitoringEnabled.Should().Be(enabled);
        domain.IsActive.Should().Be(!enabled);

        _repository.Verify(x => x.Update(domain), Times.Once);
        _mapper.Verify(x => x.Map<DomainResponse>(domain), Times.Once);
    }

    /// <summary>
    /// Tests that updating a non-existent domain throws <see cref="DomainNotFoundException"/> and does not update.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainDoesNotExist_ThrowsAndDoesNotUpdate()
    {
        // Arrange
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .EnableMonitoring()
            .BuildUpdateCommand();
        
        // Act
        _ownedDomains
            .Setup(x => x.GetRequiredAsync(
                _fakeDomainId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DomainNotFoundException(_fakeDomainId));

        var action = () => _handler.Handle(
            command,
            CancellationToken.None
            );

        // Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();

        _repository.Verify(
            x => x.Update(It.IsAny<DomainEntity>()),
            Times.Never);

        _mapper.Verify(x => x.Map<DomainResponse>(It.IsAny<DomainEntity>()),
            Times.Never);
    }
}
