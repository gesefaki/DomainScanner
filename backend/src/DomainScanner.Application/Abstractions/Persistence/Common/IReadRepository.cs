namespace DomainScanner.Application.Abstractions.Persistence;

public interface IReadRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct);
    Task<T?> FindAsync(Guid id, CancellationToken ct);
}