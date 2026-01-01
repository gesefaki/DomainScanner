using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Commands.DisableUser;

public record DeactivateUserCommand(Guid Id) : IRequest<Guid>;