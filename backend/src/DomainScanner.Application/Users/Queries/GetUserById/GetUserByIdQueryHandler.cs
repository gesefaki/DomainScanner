using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace DomainScanner.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUsersRepository _usersRepository;
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(IUsersRepository usersRepository,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _usersRepository = usersRepository;
        _logger = logger;
    }
    
    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        _logger.LogInformation($"Getting user with id {request.Id}...");
        var user =  await _usersRepository.GetUserByIdAsync(request.Id,  ct);

        // null's because we can get null from GetUserByIdAsync()
        _logger.LogInformation($"Founded: User with id {user?.Id ?? null}: {user?.Username ?? null}");
        return user;
    }
}