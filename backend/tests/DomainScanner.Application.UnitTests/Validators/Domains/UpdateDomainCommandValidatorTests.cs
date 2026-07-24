using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Domains;

/// <summary>
/// Unit tests for <see cref="UpdateDomainCommandValidator"/>.
/// </summary>
public class UpdateDomainCommandValidatorTests
{
    private readonly UpdateDomainCommandValidator _validator = new();

    [Fact]
    public void Validate_WhenRequestIsValid_HasNoErrors()
    {
        var command = new UpdateDomainCommand(
            Guid.NewGuid(), new UpdateDomainRequest("http://example.com", true));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenAddressViolatesCommonRules_HasAddressError()
    {
        var command = new UpdateDomainCommand(
            Guid.NewGuid(), new UpdateDomainRequest("ftp://example.com", true));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.Address)
            .WithErrorMessage("Domain URL must be a valid HTTP or HTTPS URL.");
    }
}
