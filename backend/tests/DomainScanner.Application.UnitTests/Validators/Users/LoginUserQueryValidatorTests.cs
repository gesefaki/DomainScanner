using DomainScanner.Application.Handlers.Users.Queries.LoginUser;
using DomainScanner.Contracts.DTOs.Users.Requests;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Users;

/// <summary>
/// Unit tests for <see cref="LoginUserQueryValidator"/>.
/// </summary>
public class LoginUserQueryValidatorTests
{
    private readonly LoginUserQueryValidator _validator = new();
    
    /// <summary>
    /// Tests that valid passwords (8+ characters) pass validation.
    /// </summary>
    /// <param name="validPassword">A password with minimum 8 characters.</param>
    [Theory]
    [InlineData("12345678")]
    [InlineData("abcdefgh")]
    [InlineData("Password")]
    [InlineData("pass1234")]
    [InlineData("P@ssw0rd")]
    [InlineData("123456789")]
    [InlineData("MyPass123")]
    [InlineData("SecurePass")]
    public void Validate_WhenPasswordLengthIsValid_HasNoPasswordError(string validPassword)
    {
        // Arrange
        var query = CreateQuery(validPassword);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Password);
    }
    
    /// <summary>
    /// Tests that invalid passwords (less than 8 characters) fail validation.
    /// </summary>
    /// <param name="invalidPassword">A password with less than 8 characters.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234567")]
    [InlineData("passwor")]
    [InlineData("Pass123")]
    [InlineData("P@ssw0r")]
    [InlineData("12345")]
    [InlineData("abc")]
    [InlineData("P1")]
    public void Validate_WhenPasswordLengthIsInvalid_ShouldHaveErrors(string invalidPassword)
    {
        // Arrange
        var query = CreateQuery(invalidPassword);
        
        // Act
        var result = _validator.TestValidate(query);
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Password);
    }
    
    /// <summary>
    /// Creates a test query with default email and specified password.
    /// </summary>
    /// <param name="password">The password to test.</param>
    /// <returns>A new <see cref="LoginUserQuery"/> instance.</returns>
    private static LoginUserQuery CreateQuery(string password)
    {
        return new LoginUserQuery(new LoginUserRequest(
            "email@email.com",
            password
        ));
    }
}