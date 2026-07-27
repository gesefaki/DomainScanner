using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Users.Commands.DeleteUser;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteUserCommandHandler"/>.
/// </summary>
public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IRepository<User, Guid>> _repository = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly DeleteUserCommandHandler _handler;

    public DeleteUserCommandHandlerTests()
    {
        _handler = new DeleteUserCommandHandler(
            _repository.Object,
            _currentUser.Object);
    }

    /// <summary>
    /// Deletes an existing user and returns its identifier.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserExists_DeletesAndReturnsId()
    {
        // Arrange
        var user = new UserBuilder().Build();
        _currentUser.SetupGet(x => x.Id).Returns(user.Id);
        
        _repository.SetupFindAsync(user.Id, user);

        // Act
        var result = await _handler.Handle(new DeleteUserCommand(), CancellationToken.None);

        // Assert
        result.Should().Be(user.Id);
        _repository.Verify(x => x.Delete(user), Times.Once);
    }

    /// <summary>
    /// Does not delete and throws when the user cannot be found.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.Id).Returns(id);
        _repository.SetupFindAsync(id, (User?)null);

        // Act
        var action = () => _handler.Handle(new DeleteUserCommand(), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();
        _repository.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
    }
}
