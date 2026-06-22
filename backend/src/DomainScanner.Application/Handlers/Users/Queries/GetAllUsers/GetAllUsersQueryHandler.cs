using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler: IRequestHandler<GetAllUsersQuery, List<User>>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(IUsersRepository usersRepository,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }
    
    public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        _logger.LogInformation("Getting all users...");
        var users  = await _usersRepository.GetAllUsersAsync(ct);
        
        _logger.LogInformation($"Found {users.Count} users.");
        return users;
    }
}