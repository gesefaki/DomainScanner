using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.CreateDomain;

public record CreateDomainCommand(DomainEntity Domain) : IRequest<Guid>
{
}