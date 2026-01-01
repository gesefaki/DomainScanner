using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IUsersRepository
{
    Task<List<User>> GetAllUsersAsync(CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid id,  CancellationToken ct);
    Task CreateUserAsync(User user,  CancellationToken ct);
    void DeleteUser(User user);
    void UpdateUser(User user);
}