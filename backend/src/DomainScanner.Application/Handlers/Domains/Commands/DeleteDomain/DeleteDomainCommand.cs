using DomainScanner.Application.Pipelines.Interfaces;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

public record DeleteDomainCommand(Guid Id) : ICommand<Unit>;