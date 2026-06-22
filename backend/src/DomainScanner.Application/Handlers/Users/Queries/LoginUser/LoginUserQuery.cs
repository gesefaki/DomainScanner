using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

public record LoginUserQuery(string Email, string Password) : IRequest<string>;