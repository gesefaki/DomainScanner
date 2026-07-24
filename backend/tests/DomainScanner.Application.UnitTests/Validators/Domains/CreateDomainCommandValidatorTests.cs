using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using FluentValidation.TestHelper;

namespace DomainScanner.Application.UnitTests.Validators.Domains;

/// <summary>
/// Unit tests for <see cref="CreateDomainCommandValidator"/>.
/// </summary>
public class CreateDomainCommandValidatorTests
{
    private readonly CreateDomainCommandValidator _validator;

    private readonly Guid _fakeUserId = Guid.NewGuid();

    public CreateDomainCommandValidatorTests()
    {
        _validator = new CreateDomainCommandValidator();
    }
    [Fact]
    public void Validate_WhenUserIdIsEmpty_HasUserIdError()
    {
        var command = new CreateDomainCommand(
            new CreateDomainRequest("https://example.com", Guid.Empty));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Request.UserId)
            .WithErrorMessage("User ID is required.");
    }

    [Fact]
    public void Validate_WhenRequestIsValid_HasNoErrors()
    {
        var command = new CreateDomainCommand(
            new CreateDomainRequest("https://example.com", _fakeUserId));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
