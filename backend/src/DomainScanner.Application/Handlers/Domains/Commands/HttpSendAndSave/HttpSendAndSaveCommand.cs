using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

public record HttpSendAndSaveCommand(Guid Id) : ICommand<DomainCheckResult>;