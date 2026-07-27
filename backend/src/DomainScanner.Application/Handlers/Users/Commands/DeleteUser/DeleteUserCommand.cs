using DomainScanner.Application.Pipelines.Interfaces;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete the authenticated user.
/// </summary>
public record DeleteUserCommand : ICommand<Guid>, INeedAuthentication;
