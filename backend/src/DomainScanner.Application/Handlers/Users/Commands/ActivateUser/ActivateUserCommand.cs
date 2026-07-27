using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Responses;

namespace DomainScanner.Application.Handlers.Users.Commands.ActivateUser;

/// <summary>
/// Command to activate the authenticated user.
/// </summary>
public record ActivateUserCommand : ICommand<UserResponse>, INeedAuthentication;
