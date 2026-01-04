using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _uof;
    private readonly ILogger<DeleteUserCommandHandler> _logger;

    public DeleteUserCommandHandler(IUsersRepository usersRepository,
        IUnitOfWork uof,
        ILogger<DeleteUserCommandHandler> logger)
    {
        _usersRepository = usersRepository;
        _uof = uof;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(DeleteUserCommand request, CancellationToken ct)
    {
        // Getting user
        _logger.LogInformation($"Getting user with id {request.Id}...");
        var user = await _usersRepository.GetUserByIdAsync(request.Id, ct);
        if (user is null)
        {
            _logger.LogWarning($"User with id {request.Id} not found.");
            throw new UserNotFoundException(nameof(User), request.Id);
        }

        _logger.LogInformation($"User  with id {request.Id} was found.");

        // Deleting and save
        _logger.LogInformation($"Deleting user with id {request.Id}...");
        _usersRepository.Delete(user);
        await _uof.SaveChangesAsync(ct);
        
        _logger.LogInformation($"User with id {request.Id} was deleted.");

        return user.Id;
    }
}