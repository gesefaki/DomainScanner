using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Commands.CreateUser;

public record CreateUserCommand(User User) : IRequest<Guid>;