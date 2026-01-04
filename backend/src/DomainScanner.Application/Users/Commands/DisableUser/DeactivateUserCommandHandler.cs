using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Commands.DisableUser;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _uof;
    private readonly ILogger<DeactivateUserCommandHandler> _logger;

    public DeactivateUserCommandHandler(IUsersRepository usersRepository,
        IUnitOfWork uof,
        ILogger<DeactivateUserCommandHandler> logger)
    {
        _usersRepository = usersRepository;
        _uof = uof;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(DeactivateUserCommand request, CancellationToken ct)
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

        // Is user deactivated?
        if (!user.IsActive)
        {
            _logger.LogError($"User with id {request.Id} already deactivated.");
            throw new UnableToExecuteException(nameof(user), user.Id, nameof(user.IsActive));
        }

        // Deactivating
        _logger.LogInformation($"Deactivating user with id {request.Id}...");
        
        user.IsActive = false;
        _usersRepository.Update(user);
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation($"User with id {request.Id} was deactivated.");

        return user.Id;
    }
}