using FluentValidation;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

/// <summary>
/// Validates the <see cref="CreateDomainCommand"/>.
/// Implements FluentValidation's <see cref="AbstractValidator{T}"/>.  
/// </summary>
public class CreateDomainCommandValidator : AbstractValidator<CreateDomainCommand>
{
    /// <summary>
    /// Sets up all validation rules for the <see cref="CreateDomainCommand"/>. 
    /// </summary>
    public CreateDomainCommandValidator()
    {
        RuleFor(d => d.Request.Address)
            .NotEmpty()
            .WithMessage("Domain address is required")
            .MaximumLength(150)
            .WithMessage("Domain maximum length must not exceed 100 characters")
            .Must(IsValidUrl)
            .WithMessage("Domain URL must be valid.");
    }
    
    /// <summary>
    /// Validates that the domain address is a valid URL.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}