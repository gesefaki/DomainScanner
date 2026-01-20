using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IUsersRepository
{
    Task<List<User>> GetAllUsersAsync(CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid id,  CancellationToken ct);
    Task CreateAsync(User user,  CancellationToken ct);
    Task<bool> IsExistsByEmailAsync(string email, CancellationToken ct);
    void Delete(User user);
    void Update(User user);
}