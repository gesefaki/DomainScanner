using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Queries.GetAllUsers;

public record GetAllUsersQuery() : IRequest<List<User>>;