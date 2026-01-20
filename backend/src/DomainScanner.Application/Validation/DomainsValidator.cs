using DomainScanner.Domain.Entities;
using FluentValidation;

namespace DomainScanner.Application.Validation;

public class DomainsValidator : AbstractValidator<DomainEntity>
{
    public DomainsValidator()
    {
        RuleFor(d => d.Address)
            .NotEmpty().WithMessage("The address cannot be empty");

        RuleFor(d => d)
            .Must(ValidateDomain)
            .WithMessage("The address is not valid");

    }

    private bool ValidateDomain(DomainEntity domain)
    {
        if ( string.IsNullOrEmpty(domain.Address) ) return false;
        if ( !domain.Address.StartsWith("http") || !domain.Address.StartsWith("https") ) return false;
        if ( !domain.Address.Contains(".") ) return false;
        if ( domain.AddressToUri() is null ) return false;
        return true;
    }
}