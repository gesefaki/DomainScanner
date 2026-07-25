using DomainScanner.Application.Handlers.Domains.Common;
using FluentValidation;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Domains;

/// <summary>
/// Unit tests for <see cref="DomainValidationRules"/>.
/// </summary>
public class DomainValidationRulesTests
{
    private readonly AddressValidator _validator = new();
    
    /// <summary>
    /// Tests that valid HTTP/HTTPS URLs pass validation.
    /// </summary>
    /// <param name="address">A valid HTTP or HTTPS URL.</param>
    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/path?query=value")]
    public void Validate_WhenHttpUrlIsValid_HasNoAddressError(string address)
    {
        // Act
        var result = _validator.TestValidate(new AddressModel(address));
        
        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Address);
    }
    
    /// <summary>
    /// Tests that invalid URLs fail validation.
    /// </summary>
    /// <param name="address">An invalid URL or unsupported scheme.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("example.com")]
    [InlineData("ftp://example.com")]
    public void Validate_WhenAddressIsInvalid_HasAddressError(string address)
    {
        // Act
        var result = _validator.TestValidate(new AddressModel(address));
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }
    
    /// <summary>
    /// Tests that addresses exceeding 150 characters fail validation.
    /// </summary>
    [Fact]
    public void Validate_WhenAddressExceeds150Characters_HasMaximumLengthError()
    {
        // Arrange
        var address = "https://" + new string('a', 139) + ".com";
        
        // Act
        var result = _validator.TestValidate(new AddressModel(address));
        
        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address)
            .WithErrorMessage("Domain address must not exceed 150 characters.");
    }
    
    /// <summary>
    /// Test model wrapping an address string for validation.
    /// </summary>
    private sealed record AddressModel(string Address);
    
    
    /// <summary>
    /// Validator for <see cref="AddressModel"/>.
    /// </summary>
    private sealed class AddressValidator : AbstractValidator<AddressModel>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Address).ValidDomainAddress();
        }
    }
}
