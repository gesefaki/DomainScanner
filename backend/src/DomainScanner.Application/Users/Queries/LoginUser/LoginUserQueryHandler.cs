using DomainScanner.Application.Abstractions;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Queries.LoginUser;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, string>
{
    private readonly ILogger <LoginUserQueryHandler> _logger;
    private readonly IUsersRepository _repository;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwtProvider;

    public LoginUserQueryHandler(ILogger<LoginUserQueryHandler> logger, IUsersRepository repository, 
        IPasswordHasher hasher, IJwtProvider jwtProvider)
    {
        _logger = logger;
        _repository = repository;
        _hasher = hasher;
        _jwtProvider = jwtProvider;
    }
    
    public async Task<string> Handle(LoginUserQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Attempting to get user with email {email}", request.Email);
        var user = await _repository.GetUserByEmailAsync(request.Email, ct);

        if (user is null || !_hasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for email {email}",  request.Email);
            throw new InvalidCredentialsException();    
        }

        var token = _jwtProvider.GenerateToken(user);
        return token;
    }
    
}