using System.Security.Authentication;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, string>
{
    private readonly IReadRepository<User> _readRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserQueryHandler(IReadRepository<User> readRepository,
        IPasswordHasher hasher,
        IJwtProvider jwtProvider)
    {
        _readRepository = readRepository;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }

    public async Task<string> Handle(LoginUserQuery request, CancellationToken ct)
    {
        var user = await _readRepository.GetAsync(u => u.Email == request.Email, ct);

        if (user is null)
        {
            throw new UserNotFoundException(request.Email);
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialException();
        }

        var token = _jwtProvider.GenerateToken(user);
        return token;
    }

}