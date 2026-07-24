using FluentValidation;

namespace DomainScanner.Application.Handlers.Domains.Common;

public static class DomainValidationRules 
{
    public static IRuleBuilderOptions<T, string> ValidDomainAddress<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            //Empty
            .NotEmpty()
            .WithMessage("Domain address is required.")
            
            // Length
            .MaximumLength(150)
            .WithMessage("Domain address must not exceed 150 characters.")
            
            // Valid scheme
            .Must(address =>
                Uri.TryCreate(address, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Domain URL must be a valid HTTP or HTTPS URL.");
    }
}