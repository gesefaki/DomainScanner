using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.UpdateDomain;

public record UpdateDomainCommand(Guid Id, DomainEntity Domain) : IRequest<Guid>;