using Microsoft.EntityFrameworkCore.Storage;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
    void Attach<T>(T entity) where T : class;
}