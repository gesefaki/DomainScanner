using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;

public record DeleteDomainCommand(DeleteDomainRequest Request) : ICommand<Unit>;