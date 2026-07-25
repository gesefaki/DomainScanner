using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Domains;

/// <summary>
/// Unit tests for <see cref="DeleteDomainCommandHandler"/>.
/// </summary>
public class DeleteDomainCommandHandlerTests
{
    private readonly Mock<IRepository<DomainEntity, Guid>> _repository = new();
    private readonly DeleteDomainCommandHandler _handler;

    private readonly Guid _fakeDomainId = Guid.NewGuid();
    
    public DeleteDomainCommandHandlerTests()
    {
        _handler = new DeleteDomainCommandHandler(_repository.Object);
    }
    
    /// <summary>
    /// Tests that an existing domain is successfully deleted.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainExists_DeletesIt()
    {
        // Arrange
        var domain = new DomainBuilder()
            .WithId(_fakeDomainId)
            .Build();
        
        var command = new DeleteDomainCommand(_fakeDomainId);
        
        _repository.SetupFindAsync(_fakeDomainId, domain);

        _repository
            .Setup(x => x.Delete(domain));
        
        // Act
        await _handler.Handle(command, CancellationToken.None);
        
        // Assert
        _repository.Verify(x => x.Delete(domain),
            Times.Once);
    }

    /// <summary>
    /// Tests that deleting a non-existent domain throws <see cref="DomainNotFoundException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_WhenDomainDoesNotExists_Throw()
    {
        // Arrange
        var command = new DomainCommandBuilder()
            .WithId(_fakeDomainId)
            .BuildDeleteCommand();

        _repository.SetupFindAsync(_fakeDomainId, (DomainEntity?)null);
        
        // Act
        var action = () => _handler.Handle(
            command,
            CancellationToken.None);
        
        // Assert
        await action.Should().ThrowAsync<DomainNotFoundException>();
        
        _repository.Verify(x => x.Delete(It.IsAny<DomainEntity>()),
            Times.Never);
    }
}