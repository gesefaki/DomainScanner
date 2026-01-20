using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Commands.HttpSendAndSave;

public record HttpSendAndSaveCommand(Guid Id) : IRequest<DomainCheckResult>;