using System.Security.Authentication;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.LoginUser;

/// <summary>
/// Handles <see cref="LoginUserQuery"/>. Has a <see cref="LoginUserQueryValidator"/> must be passed. 
/// </summary>
public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, string>
{
    private readonly IReadRepository<User, Guid> _readRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserQueryHandler(IReadRepository<User, Guid> readRepository,
        IPasswordHasher hasher,
        IJwtProvider jwtProvider)
    {
        _readRepository = readRepository;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }

    /// <inheritdoc />
    public async Task<string> Handle(LoginUserQuery request, CancellationToken ct)
    {
        var user = await _readRepository.GetAsync(u => u.Email == request.Request.Email, ct);

        if (user is null)
        {
            throw new UserNotFoundException(request.Request.Email);
        }

        var existsByEmail = await _readRepository.IsExistsByAttribute(u => u.Email == request.Request.Email, ct);

        if (existsByEmail || !_hasher.Verify(request.Request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialException();
        }

        var token = _jwtProvider.GenerateToken(user);
        return token;
    }

}