using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Domains.Commands.DeleteDomain;

public record DeleteDomainCommand(Guid Id) : IRequest<Guid>;