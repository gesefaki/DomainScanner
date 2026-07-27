using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;
using DomainScanner.Application.UnitTests.TestData.Mocks;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Commands;

/// <summary>
/// Unit tests for <see cref="DeactivateUserCommandHandler"/>.
/// </summary>
public class DeactivateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly DeactivateUserCommandHandler _handler;

    public DeactivateUserCommandHandlerTests()
    {
        _handler = new DeactivateUserCommandHandler(
            _repository.Object,
            _mapper.Object,
            _currentUser.Object);
    }

    /// <summary>
    /// Deactivates an active user and returns its mapped response.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsActive_DeactivatesAndReturnsResponse()
    {
        // Arrange
        var user = new UserBuilder()
            .Active()
            .Build();
        _currentUser.SetupGet(x => x.Id).Returns(user.Id);
        
        var response = new UserResponseBuilder()
            .WithId(user.Id)
            .WithUsername(user.Username)
            .WithEmail(user.Email)
            .Inactive()
            .Build();
        
        _repository.SetupFindAsync(user.Id, user);
        _repository.Setup(x => x.Update(user)).Returns(user);
        _mapper.Setup(x => x.Map<UserResponse>(user)).Returns(response);

        // Act
        var result = await _handler.Handle(new DeactivateUserCommand(), CancellationToken.None);

        // Assert
        result.Should().Be(response);
        user.IsActive.Should().BeFalse();
        _repository.Verify(x => x.Update(user), Times.Once);
    }

    /// <summary>
    /// Throws when the user cannot be found.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserDoesNotExist_ThrowsUserNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _currentUser.SetupGet(x => x.Id).Returns(id);
        _repository.SetupFindAsync(id, (User?)null);

        // Act
        var action = () => _handler.Handle(new DeactivateUserCommand(), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();
        _repository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    /// <summary>Throws when an inactive user is deactivated again.</summary>
    [Fact]
    public async Task Handle_WhenUserIsAlreadyInactive_ThrowsUnableToExecuteException()
    {
        // Arrange
        var user = new UserBuilder()
            .Inactive()
            .Build();
        _currentUser.SetupGet(x => x.Id).Returns(user.Id);
        _repository.SetupFindAsync(user.Id, user);

        // Act
        var action = () => _handler.Handle(new DeactivateUserCommand(), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnableToExecuteException>();
        _repository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }
}
