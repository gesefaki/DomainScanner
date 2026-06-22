using FluentValidation;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

public class CreateDomainCommandValidator : AbstractValidator<CreateDomainCommand>
{
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
    
    private bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}