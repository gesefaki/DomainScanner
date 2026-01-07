using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class UsersRepository(ScannerDbContext context) : IUsersRepository
{
    private readonly ScannerDbContext _context = context;

    public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var users = _context.Users
            .Include(u => u.Domains)
            .ThenInclude(d => d.CheckResults)
            .OrderBy(u => u.Username)
            .AsSplitQuery();
        
        return await users.ToListAsync(cancellationToken);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.Domains)
            .ThenInclude(d => d.CheckResults)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user;
    }

    public async Task CreateAsync(User user, CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);
    
    public void Delete(User user)
        => _context.Users.Remove(user);

    public void Update(User user)
        => _context.Users.Update(user);
}