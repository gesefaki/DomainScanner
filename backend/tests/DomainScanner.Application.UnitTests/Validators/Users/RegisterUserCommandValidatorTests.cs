using System.Linq.Expressions;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Contracts.DTOs.Users.Requests;
using DomainScanner.Domain.Entities;
using FluentValidation.TestHelper;
using Moq;

namespace DomainScanner.Application.UnitTests.Validators.Users;

/// <summary>
/// Unit tests for <see cref="RegisterUserCommandValidator"/>.
/// </summary>
public class RegisterUserCommandValidatorTests
{
    private readonly Mock<IReadRepository<User, Guid>> _usersRepository = new();
    private readonly Mock<IEmailNormalizer> _emailNormalizer = new();
    
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator(
            _usersRepository.Object,
            _emailNormalizer.Object);
    }
    
    /// <summary>
    /// Tests that validation fails when email is already registered.
    /// </summary>
    [Fact]
    public async Task Validate_WhenEmailAlreadyRegistered_ReturnsEmailError()
    {
        // Arrange
        _usersRepository
            .Setup(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = CreateCommand();
        
        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Email)
            .WithErrorMessage("Email already registered.");

        _usersRepository.Verify(x => x.IsExistsByAttribute(
            It.IsAny<Expression<Func<User, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    /// <summary>
    /// Tests that validation passes when email is free and all fields are valid.
    /// </summary>
    [Fact]
    public async Task Validate_WhenEmailIsFreeAndCommandIsValid_HasNoErrors()
    {
        // Arrange
        _usersRepository
            .Setup(x => x.IsExistsByAttribute(
                It.IsAny<Expression<Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = CreateCommand();

        // Act
        var result = await _validator.TestValidateAsync(command);
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();

    }
    
    /// <summary>
    /// Creates a test command with default or custom values.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="email">The email address.</param>
    /// <param name="password">The password.</param>
    /// <returns>A new <see cref="RegisterUserCommand"/> instance.</returns>
    private static RegisterUserCommand CreateCommand(
        string username = "test_user",
        string email = "user@example.com",
        string password = "Password1")
    {
        return new RegisterUserCommand(
            new RegisterUserRequest(
                username,
                email,
                password));
    }
}