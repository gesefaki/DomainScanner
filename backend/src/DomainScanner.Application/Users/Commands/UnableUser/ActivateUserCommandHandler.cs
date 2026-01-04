using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Commands.UnableUser;

public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _uof;
    private readonly ILogger<ActivateUserCommandHandler> _logger;

    public ActivateUserCommandHandler(IUsersRepository usersRepository,
        IUnitOfWork uof,
        ILogger<ActivateUserCommandHandler> logger)
    {
        _usersRepository = usersRepository;
        _uof = uof;
        _logger = logger;
    }

    public async Task<Guid> Handle(ActivateUserCommand request, CancellationToken ct)
    {
        // Getting user
        _logger.LogInformation($"Getting user with id {request.Id}...");
        var user = await _usersRepository.GetUserByIdAsync(request.Id, ct);
        if (user is null)
        {
            _logger.LogWarning($"User with id {request.Id} not found.");
            throw new UserNotFoundException(nameof(User), request.Id);
        }
        _logger.LogInformation($"User with id {request.Id} was found.");
        
        // Is user activated?
        if (user.IsActive)
        {
            _logger.LogError($"User with id {request.Id} is already activated.");
            throw new UnableToExecuteException(nameof(user), request.Id, nameof(user.IsActive));
        }

        // Activation
        _logger.LogInformation($"Activating user with id {request.Id}...");
        
        user.IsActive = true;
        _usersRepository.Update(user);
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation($"User with id {request.Id} was activated.");
        
        return user.Id;
    }
}