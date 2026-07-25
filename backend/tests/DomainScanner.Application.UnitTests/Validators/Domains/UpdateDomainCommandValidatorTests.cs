using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.UnitTests.TestData;
using DomainScanner.Application.UnitTests.TestData.Domains;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Domains;

/// <summary>
/// Unit tests for <see cref="UpdateDomainCommandValidator"/>.
/// </summary>
public class UpdateDomainCommandValidatorTests
{
    private readonly UpdateDomainCommandValidator _validator = new();
    
    /// <summary>
    /// Tests that validation passes when all fields are valid.
    /// </summary>
    [Fact]
    public void Validate_WhenRequestIsValid_HasNoErrors()
    {
        var command = new DomainCommandBuilder()
            .BuildUpdateCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
    
    /// <summary>
    /// Tests that validation fails when address is invalid (unsupported scheme).
    /// </summary>
    [Fact]
    public void Validate_WhenAddressViolatesCommonRules_HasAddressError()
    {
        var command = new DomainCommandBuilder()
            .WithAddress("ftp://example.com")
            .BuildUpdateCommand();
        
        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Address)
            .WithErrorMessage("Domain URL must be a valid HTTP or HTTPS URL.");
    }
}
