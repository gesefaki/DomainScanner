using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Commands.UnableUser;

public record ActivateUserCommand(Guid Id) : IRequest<Guid>;