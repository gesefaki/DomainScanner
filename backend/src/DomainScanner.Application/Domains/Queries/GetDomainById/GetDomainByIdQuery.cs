using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Domains.Queries.GetDomainById;

public record GetDomainByIdQuery(Guid Id) : IRequest<DomainEntity?>;