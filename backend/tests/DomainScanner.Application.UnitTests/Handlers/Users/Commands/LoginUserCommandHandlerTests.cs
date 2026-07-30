using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Auth.Models;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Commands.LoginUser;
using DomainScanner.Application.UnitTests.TestData.Users;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using FluentAssertions;
using Moq;

namespace DomainScanner.Application.UnitTests.Handlers.Users.Commands;

/// <summary>
/// Unit tests for <see cref="LoginUserCommandHandler"/>.
/// </summary>
public class LoginUserCommandHandlerTests
{
    private readonly Mock<IReadRepository<User, Guid>> _repository = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IEmailNormalizer> _emailNormalizer = new();
    private readonly Mock<ILoginAccountKeyProvider> _accountKeyProvider = new();
    private readonly Mock<ILoginAttemptProtector> _loginAttemptProtector = new();
    
    private readonly LoginUserCommandHandler _handler;

    private const string FakeToken = "jwt-token";
    private const string RawEmail = " User@Example.COM ";
    private const string NormalizedEmail = "user@example.com";
    private const string AccountKey = "account:user@example.com";

    public LoginUserCommandHandlerTests()
    {
        _emailNormalizer
            .Setup(x => x.Normalize(It.IsAny<string>()))
            .Returns((string email) => email.Trim().ToLowerInvariant());

        _accountKeyProvider
            .Setup(x => x.Create(It.IsAny<string>()))
            .Returns((string normalizedEmail) => $"account:{normalizedEmail}");

        _loginAttemptProtector
            .Setup(x => x.GetStateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginAttemptState(
                IsBlocked: false,
                FailedAttempts: 0,
                RetryAfter: TimeSpan.Zero));

        _loginAttemptProtector
            .Setup(x => x.RegisterFailureAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginFailureResult(
                FailedAttempts: 1,
                IsBlocked: false,
                Delay: TimeSpan.Zero,
                RetryAfter: TimeSpan.Zero));

        _loginAttemptProtector
            .Setup(x => x.ResetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new LoginUserCommandHandler(
            _repository.Object,
            _hasher.Object,
            _jwtProvider.Object,
            _emailNormalizer.Object,
            _accountKeyProvider.Object,
            _loginAttemptProtector.Object);
    }
    
    [Fact]
    public async Task Handle_BlockedAccount_ThrowsBeforeUserLookup()
    {
        // Arrange
        var command = CreateCommand();
        var retryAfter = TimeSpan.FromMinutes(10);

        _loginAttemptProtector
            .Setup(x => x.GetStateAsync(AccountKey, CancellationToken.None))
            .ReturnsAsync(new LoginAttemptState(
                IsBlocked: true,
                FailedAttempts: 5,
                RetryAfter: retryAfter));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should()
            .ThrowAsync<LoginTemporarilyBlockedException>();

        exception.Which.RetryAfter.Should().Be(retryAfter);
        _repository.Verify(
            x => x.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        VerifyNoCredentialProcessing();
    }

    [Fact]
    public async Task Handle_UnknownEmail_RegistersFailureAndThrowsInvalidCredentials()
    {
        // Arrange
        var command = CreateCommand();

        _repository.Setup(x => x.GetAsync(
                It.IsAny<Expression<Func<User, bool>>>(),
                CancellationToken.None))
            .ReturnsAsync((User?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserInvalidCredentialsException>();
        _loginAttemptProtector.Verify(
            x => x.RegisterFailureAsync(AccountKey, CancellationToken.None),
            Times.Once);
        _hasher.Verify(
            x => x.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _loginAttemptProtector.Verify(
            x => x.ResetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtProvider.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_InvalidPassword_RegistersFailureAndThrowsInvalidCredentials()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();
        SetupUserLookup(user, CancellationToken.None);

        _hasher
            .Setup(x => x.Verify(command.Request.Password, user.PasswordHash))
            .Returns(false);

        _loginAttemptProtector
            .Setup(x => x.RegisterFailureAsync(
                AccountKey,
                CancellationToken.None))
            .ReturnsAsync(new LoginFailureResult(
                FailedAttempts: 3,
                IsBlocked: false,
                Delay: TimeSpan.FromMilliseconds(1),
                RetryAfter: TimeSpan.Zero));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<UserInvalidCredentialsException>();
        _loginAttemptProtector.Verify(
            x => x.RegisterFailureAsync(AccountKey, CancellationToken.None),
            Times.Once);
        _loginAttemptProtector.Verify(
            x => x.ResetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtProvider.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_FailureReachesLockoutThreshold_ThrowsBlockedException()
    {
        // Arrange
        var command = CreateCommand();
        var user = CreateUser();
        var retryAfter = TimeSpan.FromMinutes(10);
        SetupUserLookup(user, CancellationToken.None);

        _hasher
            .Setup(x => x.Verify(command.Request.Password, user.PasswordHash))
            .Returns(false);

        _loginAttemptProtector
            .Setup(x => x.RegisterFailureAsync(
                AccountKey,
                CancellationToken.None))
            .ReturnsAsync(new LoginFailureResult(
                FailedAttempts: 5,
                IsBlocked: true,
                Delay: TimeSpan.Zero,
                RetryAfter: retryAfter));

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should()
            .ThrowAsync<LoginTemporarilyBlockedException>();

        exception.Which.RetryAfter.Should().Be(retryAfter);
        _loginAttemptProtector.Verify(
            x => x.ResetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtProvider.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ResponseDelayIsCancelled_PropagatesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var command = CreateCommand();
        var user = CreateUser();
        SetupUserLookup(user, cts.Token);

        _hasher
            .Setup(x => x.Verify(command.Request.Password, user.PasswordHash))
            .Returns(false);

        _loginAttemptProtector
            .Setup(x => x.RegisterFailureAsync(AccountKey, cts.Token))
            .ReturnsAsync(new LoginFailureResult(
                FailedAttempts: 3,
                IsBlocked: false,
                Delay: TimeSpan.FromMinutes(1),
                RetryAfter: TimeSpan.Zero));

        await cts.CancelAsync();

        // Act
        var action = () => _handler.Handle(command, cts.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
        _loginAttemptProtector.Verify(
            x => x.RegisterFailureAsync(AccountKey, cts.Token),
            Times.Once);
        _jwtProvider.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }
    
    [Fact]
    public async Task Handle_ValidCredentials_ResetsFailuresAndReturnsToken()
    {
        // Arrange
        var ct = CancellationToken.None;
        var command = CreateCommand();
        var user = CreateUser();
        SetupUserLookup(user, ct);

        _loginAttemptProtector.Setup(x =>
                x.GetStateAsync(AccountKey, ct))
            .ReturnsAsync(new LoginAttemptState(
                IsBlocked: false,
                FailedAttempts: 0,
                RetryAfter: TimeSpan.Zero));

        _loginAttemptProtector.Setup(x =>
                x.ResetAsync(AccountKey, ct))
            .Returns(Task.CompletedTask);
        
        _hasher.Setup(x => x.Verify(command.Request.Password, user.PasswordHash))
            .Returns(true);
        
        _jwtProvider.Setup(x => x.GenerateToken(user))
            .Returns(FakeToken);

        // Act
        var result = await _handler.Handle(command, ct);

        // Assert
        result.Should().Be(FakeToken);

        _emailNormalizer.Verify(x => x.Normalize(RawEmail), Times.Once);
        _accountKeyProvider.Verify(x => x.Create(NormalizedEmail), Times.Once);
        _loginAttemptProtector.Verify(
            x => x.GetStateAsync(AccountKey, ct),
            Times.Once);
        _hasher.Verify(
            x => x.Verify(command.Request.Password, user.PasswordHash),
            Times.Once);
        _loginAttemptProtector.Verify(
            x => x.ResetAsync(AccountKey, ct),
            Times.Once);
        _loginAttemptProtector.Verify(
            x => x.RegisterFailureAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtProvider.Verify(x => x.GenerateToken(user), Times.Once);
    }

    private static LoginUserCommand CreateCommand()
    {
        return new UserCommandBuilder()
            .WithEmail(RawEmail)
            .BuildLoginCommand();
    }

    private static User CreateUser()
    {
        return new UserBuilder()
            .WithEmail(RawEmail)
            .WithNormalizedEmail(NormalizedEmail)
            .Build();
    }

    private void SetupUserLookup(User user, CancellationToken ct)
    {
        _repository
            .Setup(x => x.GetAsync(
                It.Is<Expression<Func<User, bool>>>(
                    predicate => predicate.Compile()(user)),
                ct))
            .ReturnsAsync(user);
    }

    private void VerifyNoCredentialProcessing()
    {
        _hasher.Verify(
            x => x.Verify(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _loginAttemptProtector.Verify(
            x => x.RegisterFailureAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _loginAttemptProtector.Verify(
            x => x.ResetAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtProvider.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }
}
