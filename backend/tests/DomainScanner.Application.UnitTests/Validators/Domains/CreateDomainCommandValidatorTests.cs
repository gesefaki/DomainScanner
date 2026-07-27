using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.UnitTests.TestData;
using DomainScanner.Application.UnitTests.TestData.Domains;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Domains;

/// <summary>
/// Unit tests for <see cref="CreateDomainCommandValidator"/>.
/// </summary>
public class CreateDomainCommandValidatorTests
{
    private readonly CreateDomainCommandValidator _validator;

    public CreateDomainCommandValidatorTests()
    {
        _validator = new CreateDomainCommandValidator();
    }
    
    /// <summary>
    /// Tests that validation passes when all fields are valid.
    /// </summary>
    [Fact]
    public void Validate_WhenRequestIsValid_HasNoErrors()
    {
        var command = new DomainCommandBuilder()
            .BuildCreateCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
