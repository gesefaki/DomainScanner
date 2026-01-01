using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetAllUsersQuery, List<User>>
{
    private readonly IUsersRepository _usersRepository = usersRepository;
    
    public async Task<List<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users  = await _usersRepository.GetAllUsersAsync(cancellationToken);
        return users;
    }
}