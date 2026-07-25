using System.Linq.Expressions;
using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Commands;

/// <summary>
/// Unit tests for <see cref="RegisterUserCommandHandler"/>.
/// </summary>
public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IRepository<User, Guid>> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IMapper> _mapper = new();
    private readonly RegisterUserCommandHandler _handler;

    private const string FakePassword = "Password1";
    private const string FakePasswordHash = "hashed-password";

    public RegisterUserCommandHandlerTests()
    {
        _handler = new RegisterUserCommandHandler(
            _repository.Object,
            _hasher.Object,
            _mapper.Object);
    }

    /// <summary>
    /// Tests that available credentials create and return a user.
    /// </summary>
    [Fact]
    public async Task Handle_RegisterUserWhenCredentialsAreAvailable_CreatesAndReturns()
    {
        // Arrange
        User? createdUser = null;
        
        var command = new UserCommandBuilder()
            .WithPassword(FakePassword)
            .BuildRegisterCommand();
        
        var expectedResponse = new UserResponseBuilder()
            .WithUsername(command.Request.Username)
            .WithEmail(command.Request.Email)
            .Build();

        _repository.SetupSequence(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(false);
        
        _hasher.Setup(x => x.Generate(FakePassword)).Returns(FakePasswordHash);
        
        _repository.Setup(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => createdUser = user)
            .ReturnsAsync((User user, CancellationToken _) => user);
        
        _mapper.Setup(x => x.Map<UserResponse>(It.IsAny<User>())).Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be(command.Request.Username);
        createdUser.Email.Should().Be(command.Request.Email);
        createdUser.PasswordHash.Should().Be(FakePasswordHash);
        createdUser.IsActive.Should().BeTrue();
        
        _hasher.Verify(x => x.Generate(FakePassword), Times.Once);
        
        _repository.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Tests that an existing email throws <see cref="UserConflictCredsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_RegisterUserWhenEmailExists_ThrowsAndDoesNotCreate()
    {
        // Arrange
        var command = new UserCommandBuilder().BuildRegisterCommand();
        _repository.Setup(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserConflictCredsException>();
        
        _hasher.Verify(x => x.Generate(It.IsAny<string>()), Times.Never);
        
        _repository.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Tests that an existing username throws <see cref="UserConflictCredsException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_RegisterUserWhenUsernameExists_ThrowsAndDoesNotCreate()
    {
        // Arrange
        var command = new UserCommandBuilder().BuildRegisterCommand();
        _repository.SetupSequence(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserConflictCredsException>();
        
        _hasher.Verify(x => x.Generate(It.IsAny<string>()), Times.Never);
        
        _repository.Verify(x => x.CreateAsync(
                It.IsAny<User>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
