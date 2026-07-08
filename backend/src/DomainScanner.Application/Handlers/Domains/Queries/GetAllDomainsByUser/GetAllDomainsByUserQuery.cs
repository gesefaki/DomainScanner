using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Queries.GetAllDomainsByUser;

/// <summary>
/// Query to retrieve all DomainEntities by by associated user id
/// </summary>
/// <param name="UserId">Associated user unique identifier.</param>
public record GetAllDomainsByUserQuery(Guid UserId) : IRequest<IEnumerable<DomainResponse>>, ICacheableQuery;