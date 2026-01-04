using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(IUsersRepository usersRepository, IUnitOfWork uof,
        ILogger<CreateUserCommandHandler> logger)
    {
        _usersRepository = usersRepository;
        _uof = uof;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating new user...");
        
        await _usersRepository.CreateAsync(request.User, ct);
        
        _logger.LogInformation("New user created.");
        
        await  _uof.SaveChangesAsync(ct);
        return request.User.Id;
    }
}