using DomainScanner.Application.Abstractions;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Application.Exceptions.Users;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, User>
{
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IPasswordHasher _hasher;
    private readonly IUsersRepository _repository;
    private readonly IUnitOfWork _uof;

    public RegisterUserCommandHandler(ILogger<RegisterUserCommandHandler> logger, IPasswordHasher hasher,
        IUsersRepository repository, IUnitOfWork uof)
    {
        _logger = logger;
        _hasher = hasher;
        _repository = repository;
        _uof = uof;
    }

    public async Task<User> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Registering user with email {email}", request.Email);

        if (await _repository.IsExistsByEmailAsync(request.Email, ct) ||
            await _repository.IsExistsByUsernameAsync(request.Username, ct))
        {
            _logger.LogInformation("Failed to register user with email {email} and username {username}", request.Email, request.Username);
            throw new UserConflictCredsException();
        }
        
        var hashedPassword = _hasher.Generate(request.Password);
        
        _logger.LogInformation("Hashed password {hashedPassword}", hashedPassword);
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = hashedPassword,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            IsActive = true
        };

        _logger.LogInformation("Created new user with id {id}", user.Id);
        
        await _repository.CreateAsync(user, ct);
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation("User with id {id} registered successfully", user.Id);

        return user;
    }
}