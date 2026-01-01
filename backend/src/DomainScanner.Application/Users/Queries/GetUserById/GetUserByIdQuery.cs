using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Queries.GetUserById;

public record GetUserByIdQuery(Guid Id) : IRequest<User?>;