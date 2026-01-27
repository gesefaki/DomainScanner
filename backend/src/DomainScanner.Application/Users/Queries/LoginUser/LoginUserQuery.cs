using DomainScanner.Application.Abstractions.Mediator;

namespace DomainScanner.Application.Users.Queries.LoginUser;

public record LoginUserQuery(string Email, string Password) : IRequest<string>;