using DomainScanner.Application.Handlers.Domains.Common;
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
        RuleFor(x => x.Request.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
        
        RuleFor(x => x.Request.Address!).ValidDomainAddress();
    }
}