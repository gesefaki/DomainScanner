using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;

public record UpdateDomainCommand(Guid Id, UpdateDomainRequest Request) : ICommand<DomainResponse>;