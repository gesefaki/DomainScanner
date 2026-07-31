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

    public LoginUserCommandHandler(
        IReadRepository<User, Guid> readRepository,
        IPasswordHasher hasher,
        IJwtProvider jwtProvider,
        IEmailNormalizer emailNormalizer, 
        ILoginAccountKeyProvider accountKeyProvider,
        ILoginAttemptProtector loginAttemptProtector)
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
        // Reduce attempt to the general form
        var normalizedEmail = _emailNormalizer.Normalize(request.Request.Email);
        var accountKey = _accountKeyProvider.Create(normalizedEmail);

        // Retrieve the status of login attempts based on the unique accountKey
        var state = await _loginAttemptProtector.GetStateAsync(accountKey, ct);

        // Subsequent torture attempts may be blocked once the entry limit has been reached; we're checking this
        if (state.IsBlocked)
        {
            throw new LoginTemporarilyBlockedException(state.RetryAfter);
        }
        
        // If the attempt isn't blocked, we try to retrieve the user based on the entered credentials
        var user = await _readRepository.GetAsync(
            candidate => candidate.NormalizedEmail == normalizedEmail,
            ct);
        
        // Verifying a password using its hash
        var passwordIsValid = user is not null &&
                              _hasher.Verify(
                                  request.Request.Password,
                                  user.PasswordHash);

        // If the password is incorrect, secure the attempt
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
        
        // If the attempt is successful, reset the attempt count
        await _loginAttemptProtector.ResetAsync(
            accountKey, ct);

        // Generate a JWT and return it to the user
        var token = _jwtProvider.GenerateToken(user!);
        return token;
    }

}
