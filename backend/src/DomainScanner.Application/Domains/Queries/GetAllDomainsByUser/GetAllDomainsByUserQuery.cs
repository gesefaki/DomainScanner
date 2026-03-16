using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Queries.GetAllDomainsByUser;

public record GetAllDomainsByUserQuery(Guid UserId) : IRequest<List<DomainEntity>>;