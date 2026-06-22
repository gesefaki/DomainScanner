using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Queries.GetAllDomains;

public record GetAllDomainsQuery : IRequest<List<DomainEntity>>;