using System.Security.Authentication;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Commands.LoginUser;

/// <summary>
/// Handles <see cref="LoginUserCommand"/>. Has a <see cref="LoginUserCommandValidator"/> must be passed. 
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly IReadRepository<User, Guid> _readRepository;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserCommandHandler(IReadRepository<User, Guid> readRepository,
        IPasswordHasher hasher,
        IJwtProvider jwtProvider)
    {
        _readRepository = readRepository;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }

    /// <inheritdoc />
    public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var user = await _readRepository.GetAsync(u => u.Email == request.Request.Email, ct);

        if (user is null || !_hasher.Verify(request.Request.Password, user.PasswordHash))
        {
            throw new UserInvalidCredentialsException();
        }

        var token = _jwtProvider.GenerateToken(user);
        return token;
    }

}
