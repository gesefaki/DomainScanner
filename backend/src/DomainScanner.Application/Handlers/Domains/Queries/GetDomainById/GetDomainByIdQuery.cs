using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;

/// <summary>
/// Query to retrieve single <see cref="DomainResponse"/> by its identifier.  
/// </summary>
/// <param name="Id">Domain unique identifier.</param>
public record GetDomainByIdQuery(Guid Id) : IRequest<DomainResponse>, ICacheableQuery;