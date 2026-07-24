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

    private sealed record AddressModel(string Address);

    private sealed class AddressValidator : AbstractValidator<AddressModel>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Address).ValidDomainAddress();
        }
    }
}
