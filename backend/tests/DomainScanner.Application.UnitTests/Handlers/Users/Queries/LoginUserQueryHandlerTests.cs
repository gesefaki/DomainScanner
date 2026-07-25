using System.Linq.Expressions;
using System.Security.Authentication;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Queries;

/// <summary>
/// Unit tests for <see cref="LoginUserQueryHandler"/>.
/// </summary>
public class LoginUserQueryHandlerTests
{
    private readonly Mock<IReadRepository<User, Guid>> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly LoginUserQueryHandler _handler;

    private const string FakeToken = "jwt-token";

    public LoginUserQueryHandlerTests()
    {
        _handler = new LoginUserQueryHandler(
            _repository.Object,
            _hasher.Object,
            _jwtProvider.Object);
    }

    /// <summary>
    /// Tests that an unknown email throws <see cref="InvalidCredentialException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_LoginUserWhenUserDoesNotExist_ThrowsInvalidCredentials()
    {
        // Arrange
        var query = new UserCommandBuilder().BuildLoginQuery();
        _repository.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidCredentialException>();
        _hasher.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtProvider.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that an incorrect password throws <see cref="InvalidCredentialException"/>.
    /// </summary>
    [Fact]
    public async Task Handle_LoginUserWhenPasswordIsInvalid_ThrowsInvalidCredentials()
    {
        // Arrange
        var user = new UserBuilder().Build();
        var query = new UserCommandBuilder().WithEmail(user.Email).BuildLoginQuery();
        _repository.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasher.Setup(x => x.Verify(query.Request.Password, user.PasswordHash)).Returns(false);

        // Act
        var action = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<InvalidCredentialException>();
        _jwtProvider.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    /// <summary>
    /// Tests that valid credentials generate and return a token.
    /// </summary>
    [Fact]
    public async Task Handle_LoginUserWhenCredentialsAreValid_ReturnsToken()
    {
        // Arrange
        var user = new UserBuilder().Build();
        
        var query = new UserCommandBuilder()
            .WithEmail(user.Email)
            .BuildLoginQuery();
        
        _repository.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        
        _hasher.Setup(x => x.Verify(query.Request.Password, user.PasswordHash)).Returns(true);
        
        _jwtProvider.Setup(x => x.GenerateToken(user)).Returns(FakeToken);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(FakeToken);
        _jwtProvider.Verify(x => x.GenerateToken(user), Times.Once);
    }
}
