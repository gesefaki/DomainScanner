using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate user.
/// </summary>
/// <param name="Id">Unique identifier of user which needs to be deactivated.</param>
public record DeactivateUserCommand(Guid Id) : ICommand<UserResponse>;