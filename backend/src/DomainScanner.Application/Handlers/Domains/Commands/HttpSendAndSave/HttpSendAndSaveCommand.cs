using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

/// <summary>
/// Command to send HTTP request and save <see cref="DomainCheckResult"/> in database. 
/// </summary>
/// <param name="Id">Unique identifier of DomainEntity which stores request address.</param>
public record HttpSendAndSaveCommand(Guid Id) : ICommand<DomainCheckResult>;