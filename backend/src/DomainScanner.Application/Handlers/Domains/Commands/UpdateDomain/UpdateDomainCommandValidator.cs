using DomainScanner.Application.Handlers.Domains.Common;
using FluentValidation;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

public class UpdateDomainCommandValidator : AbstractValidator<UpdateDomainCommand>
{
    public UpdateDomainCommandValidator()
    {
        RuleFor(x => x.Request.Address!).ValidDomainAddress();
    }
}