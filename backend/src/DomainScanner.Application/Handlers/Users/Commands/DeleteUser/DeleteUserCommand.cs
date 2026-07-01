using DomainScanner.Application.Pipelines.Interfaces;
using DomainScanner.Contracts.DTOs.Users.Requests;

namespace DomainScanner.Application.Handlers.Users.Commands.DeleteUser;

public record DeleteUserCommand(Guid Id) : ICommand<Guid>;