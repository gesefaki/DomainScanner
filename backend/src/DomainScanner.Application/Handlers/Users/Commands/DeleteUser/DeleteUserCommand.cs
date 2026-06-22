using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : IRequest<Guid>;