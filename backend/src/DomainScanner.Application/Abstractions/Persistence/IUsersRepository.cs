using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IUsersRepository : IReadRepository<User>, IWriteRepository<User>
{
    Task<User?> GetUserByIdAsync(Guid id,  CancellationToken ct);
    Task<bool> IsExistsByEmailAsync(string email, CancellationToken ct);
    Task<bool> IsExistsByUsernameAsync(string username, CancellationToken ct);

}