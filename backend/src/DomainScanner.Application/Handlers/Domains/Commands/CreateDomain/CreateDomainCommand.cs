using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

public record CreateDomainCommand(CreateDomainRequest Request) : ICommand<DomainResponse>;