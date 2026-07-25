using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains;

/// <summary>
/// Unit tests for <see cref="UpdateDomainCommandHandler"/>.
/// </summary>
public class UpdateDomainCommandHandlerTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly UpdateDomainCommandHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    private const string FakeDomainAddress = "https://example.com/";

    public UpdateDomainCommandHandlerTests()
    {
        _handler = new UpdateDomainCommandHandler(
            _repository.Object,
            _mapper.Object);
    }

    /// <summary>
    /// Tests that an existing domain is successfully updated and returns the expected response.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainExists_UpdatesItAndReturnsResponse()
    {
        // Arrange
        var domain = new DomainBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Inactive()
            .Build();
        
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Active()
            .BuildUpdateCommand();

        var expected = new DomainResponseBuilder()
            .WithId(_fakeDomainId)
            .WithAddress(FakeDomainAddress)
            .Build();

        _repository.SetupFindAsync(_fakeDomainId, domain);

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
        domain.IsActive.Should().BeTrue();

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
            .Active()
            .BuildUpdateCommand();
        
        // Act
        _repository.SetupFindAsync(_fakeDomainId, (DomainEntity?)null);

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