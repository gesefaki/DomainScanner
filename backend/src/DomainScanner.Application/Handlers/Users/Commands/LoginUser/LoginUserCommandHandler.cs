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
    private readonly IEmailNormalizer _emailNormalizer;
    private readonly ILoginAccountKeyProvider _accountKeyProvider;
    private readonly ILoginAttemptProtector _loginAttemptProtector;

    public LoginUserCommandHandler(IReadRepository<User, Guid> readRepository,
        IPasswordHasher hasher,
        IJwtProvider jwtProvider, IEmailNormalizer emailNormalizer, 
        ILoginAccountKeyProvider accountKeyProvider, ILoginAttemptProtector loginAttemptProtector)
    {
        _readRepository = readRepository;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
        _emailNormalizer = emailNormalizer;
        _accountKeyProvider = accountKeyProvider;
        _loginAttemptProtector = loginAttemptProtector;
    }

    /// <inheritdoc />
    public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var normalizedEmail = _emailNormalizer.Normalize(request.Request.Email);

        var accountKey = _accountKeyProvider.Create(normalizedEmail);

        var state = await _loginAttemptProtector.GetStateAsync(accountKey, ct);

        if (state.IsBlocked)
        {
            throw new LoginTemporarilyBlockedException(state.RetryAfter);
        }
        
        var user = await _readRepository.GetAsync(u => u.NormalizedEmail == normalizedEmail, ct);

        if (user == null)
        {
            throw new UserInvalidCredentialsException();
        }

        var passwordIsValid = _hasher.Verify(request.Request.Password, user.PasswordHash);

        if (!passwordIsValid)
        {
            var failure = await _loginAttemptProtector.RegisterFailureAsync(
                accountKey, ct);

            if (failure.IsBlocked)
            {
                throw new LoginTemporarilyBlockedException(
                    failure.RetryAfter);
            }

            if (failure.Delay > TimeSpan.Zero)
            {
                await Task.Delay(failure.Delay, ct);
            }

            throw new UserInvalidCredentialsException();
        }

        await _loginAttemptProtector.ResetAsync(
            accountKey, ct);

        var token = _jwtProvider.GenerateToken(user);
        return token;
    }

}
