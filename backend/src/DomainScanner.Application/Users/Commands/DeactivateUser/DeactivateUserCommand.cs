using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Commands.DeactivateUser;

public record DeactivateUserCommand(Guid Id) : IRequest<Guid>;