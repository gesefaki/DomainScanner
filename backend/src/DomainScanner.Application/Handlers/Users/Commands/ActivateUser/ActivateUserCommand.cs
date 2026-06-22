using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Commands.ActivateUser;

public record ActivateUserCommand(Guid Id) : IRequest<Guid>;