using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Domain.Entities;
using DomainScanner.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace DomainScanner.Infrastructure.Persistence.Repositories;

public class UsersRepository(ScannerDbContext context) : IUsersRepository
{
    private readonly ScannerDbContext _context = context;

    public async Task<List<User>> GetAllUsersAsync(CancellationToken ct)
    {
        var users = _context.Users
            .Include(u => u.Domains)
            .ThenInclude(d => d.CheckResults)
            .OrderBy(u => u.Username)
            .AsSplitQuery();
        
        return await users.ToListAsync(ct);
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Domains)
            .ThenInclude(d => d.CheckResults)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return user;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _context.Users
            .Include(u => u.Domains)
            .ThenInclude(d => d.CheckResults)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Email == email, ct);

        return user;
    }

    public async Task<bool> IsExistsByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, ct);
    }

    public async Task<bool> IsExistsByUsernameAsync(string username, CancellationToken ct)
    {
        return await _context.Users.AnyAsync(u => u.Username == username, ct);
    }

    public async Task CreateAsync(User user, CancellationToken ct)
        => await _context.Users.AddAsync(user, ct);
    
    public void Delete(User user)
        => _context.Users.Remove(user);

    public void Update(User user)
        => _context.Users.Update(user);
}