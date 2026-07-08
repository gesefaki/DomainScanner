using DomainScanner.Application.Pipelines.Interfaces;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

/// <summary>
/// Command to delete user from database.
/// </summary>
/// <param name="Id">Unique identifier of user which needs to be deleted.</param>
public record DeleteUserCommand(Guid Id) : ICommand<Guid>;