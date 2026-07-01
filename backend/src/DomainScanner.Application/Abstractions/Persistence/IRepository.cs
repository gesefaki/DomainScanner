using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Common;

namespace DomainScanner.Application.Abstractions.Persistence;

public interface IRepository<TEntity, TId> 
    : IReadRepository<TEntity, TId>, IWriteRepository<TEntity, TId> 
    where TEntity : BaseEntity
    where TId : struct;