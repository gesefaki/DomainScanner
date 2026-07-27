using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate the authenticated user.
/// </summary>
public record DeactivateUserCommand : ICommand<UserResponse>, INeedAuthentication;
