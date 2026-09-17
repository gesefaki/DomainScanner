using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;

/// <summary>
/// Query to retrieve a single <see cref="DomainResponse"/> owned by the current authenticated user.
/// </summary>
/// <param name="Id">Unique identifier of the domain to retrieve.</param>
public record GetDomainByIdQuery(Guid Id)
    : IRequest<DomainResponse>, INeedAuthentication;
