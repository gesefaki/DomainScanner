using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class UsersRepository(ScannerDbContext context) : IUsersRepository
{
    private readonly ScannerDbContext _context = context;
    
    public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
        => await _context.Users.ToListAsync(cancellationToken);

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);
    public void DeleteUser(User user)
        => _context.Users.Remove(user);

    public void UpdateUser(User user)
        => _context.Users.Update(user);
}