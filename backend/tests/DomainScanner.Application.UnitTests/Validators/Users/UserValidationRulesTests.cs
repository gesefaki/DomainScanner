using DomainScanner.Application.Handlers.Users.Common;
using FluentValidation;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Users;

/// <summary>
/// Unit tests for <see cref="UserValidationRules"/>.
/// </summary>
public class UserValidationRulesTests
{
    private readonly EmailValidator _emailValidator = new();
    private readonly PasswordValidator _passwordValidator = new();

    /// <summary>
    /// Tests that valid email addresses pass validation.
    /// </summary>
    /// <param name="validEmail">A valid email address.</param>
    [Theory]
    [InlineData("mail@mail.com")]
    [InlineData("mail1.mail2@test.test")]
    public void Validate_WhenEmailIsValid_HasNoEmailError(string validEmail)
    {
        // Act
        var result = _emailValidator.TestValidate(new EmailModel(validEmail));
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Tests that invalid email addresses fail validation.
    /// </summary>
    /// <param name="invalidEmail">An invalid email address.</param>
    [Theory]
    [InlineData("not-a-mail")]
    [InlineData("mail.mail")]
    [InlineData("@mail.com")]
    [InlineData("mail@")]
    public void Validate_WhenEmailIsInvalid_HasEmailError(string invalidEmail)
    {
        // Act
        var result = _emailValidator.TestValidate(new EmailModel(invalidEmail));
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Tests that strong passwords pass validation.
    /// </summary>
    /// <param name="strongPassword">A password meeting all strength requirements.</param>
    [Theory]
    [InlineData("Password123")]
    [InlineData("SecurePass9")]
    [InlineData("MyPassword123")]
    [InlineData("Abc123Xyz")]
    [InlineData("P@ssw0rd123")]
    [InlineData("SuperSecure2024")]
    [InlineData("HelloWorld99")]
    [InlineData("StrongP@ss2023")]
    public void Validate_WhenPasswordIsStrong_HasNoPasswordError(string strongPassword)
    {
        // Act
        var result = _passwordValidator.TestValidate(new PasswordModel(strongPassword));
        
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    /// <summary>
    /// Tests that weak passwords fail validation.
    /// </summary>
    /// <param name="notStrongPassword">A password missing one or more strength requirements.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("pass")]
    [InlineData("Password")]
    [InlineData("password123")]
    [InlineData("PASSWORD123")]
    [InlineData("Pass123")]
    [InlineData("12345678")]
    [InlineData("PASSWORD")]
    [InlineData("1234567")]
    [InlineData("pass123")]
    [InlineData("Password!")]
    [InlineData("P@ssword")]
    [InlineData("Pass1")]
    [InlineData("P1")]
    public void Validate_WhenPasswordIsNotStrong_HasPasswordError(string notStrongPassword)
    {
        // Act
        var result = _passwordValidator.TestValidate(new PasswordModel(notStrongPassword));
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
    
    /// <summary>
    /// Test model wrapping an email string for validation.
    /// </summary>
    private sealed record EmailModel(string Email);
    
    /// <summary>
    /// Test model wrapping a password string for validation.
    /// </summary>
    private sealed record PasswordModel(string Password);

    /// <summary>
    /// Validator for <see cref="EmailModel"/>.
    /// </summary>
    private sealed class EmailValidator : AbstractValidator<EmailModel>
    {
        public EmailValidator()
        {
            RuleFor(x => x.Email).ValidEmail();
        }
    }
    
    /// <summary>
    /// Validator for <see cref="PasswordModel"/>.
    /// </summary>
    private sealed class PasswordValidator : AbstractValidator<PasswordModel>
    {
        public PasswordValidator()
        {
            RuleFor(x => x.Password).StrongPassword();
        }
    }
}
