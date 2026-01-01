using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler(IUsersRepository usersRepository) : IRequestHandler<GetUserByIdQuery, User?>
{
    private readonly IUsersRepository _usersRepository = usersRepository;

    public async Task<User?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user =  await _usersRepository.GetUserByIdAsync(request.Id,  cancellationToken);
        return user;
    }
}