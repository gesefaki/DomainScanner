using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

/// <summary>
/// Command to execute and persist a background HTTP check for a monitored domain with an active owner.
/// Does not apply current HTTP user ownership checks.
/// </summary>
/// <param name="Id">Unique identifier of the domain to check.</param>
public record HttpSendAndSaveCommand(Guid Id) : ICommand<DomainCheckResult>;
