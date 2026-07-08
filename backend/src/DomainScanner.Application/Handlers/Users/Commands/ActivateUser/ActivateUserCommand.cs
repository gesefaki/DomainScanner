using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

/// <summary>
/// Command to activate user.
/// </summary>
/// <param name="Id">Unique identifier of user which needs to be activated.</param>
public record ActivateUserCommand(Guid Id) : ICommand<UserResponse>;