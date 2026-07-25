using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Users.Commands.ActivateUser;
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
/// Unit tests for <see cref="ActivateUserCommandHandler"/>.
/// </summary>
public class ActivateUserCommandHandlerTests
{
    private readonly Mock<IRepository<User, Guid>> _repository = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly ActivateUserCommandHandler _handler;

    public ActivateUserCommandHandlerTests()
    {
        _handler = new ActivateUserCommandHandler(
            _repository.Object,
            _mapper.Object
        );
    }

    /// <summary>
    /// Activates an inactive user and returns its mapped response.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsInactive_ActivatesAndReturnsResponse()
    {
        // Arrange
        var user = new UserBuilder()
            .Inactive()
            .Build();

        var command = new ActivateUserCommand(user.Id);
        
        var response = new UserResponseBuilder()
            .WithId(user.Id)
            .WithUsername(user.Username)
            .WithEmail(user.Email)
            .Active()
            .Build();
        
        _repository.SetupFindAsync(user.Id, user);
        
        _repository.Setup(x => x.Update(user)).Returns(user);
        
        _mapper.Setup(x => x.Map<UserResponse>(user)).Returns(response);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(response);
        user.IsActive.Should().BeTrue();
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
        _repository.SetupFindAsync(id, (User?)null);

        // Act
        var action = () => _handler.Handle(new ActivateUserCommand(id), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserNotFoundException>();
        _repository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Throws when an already active user is activated again.
    /// </summary>
    [Fact]
    public async Task Handle_WhenUserIsAlreadyActive_ThrowsUnableToExecuteException()
    {
        // Arrange
        var user = new UserBuilder().Active().Build();
        _repository.SetupFindAsync(user.Id, user);

        // Act
        var action = () => _handler.Handle(new ActivateUserCommand(user.Id), CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UnableToExecuteException>();
        _repository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
    }
}
