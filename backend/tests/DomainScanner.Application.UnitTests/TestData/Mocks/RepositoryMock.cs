using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Domain.Common;
using Moq;

namespace DomainScanner.Application.UnitTests.TestData.Mocks;

/// <summary>
/// Extension methods for setting up repository mocks in tests.
/// </summary>
public static class RepositoryMockExtensions 
{
    /// <summary>
    /// Sets up the FindAsync method to return a specific entity for a given ID.
    /// </summary>
    /// <typeparam name="TRepository">The repository interface type.</typeparam>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TId">The entity ID type.</typeparam>
    /// <param name="repository">The repository mock.</param>
    /// <param name="entity">The entity to return, or <c>null</c> if not found.</param>
    /// <param name="id">The entity ID to search for.</param>
    /// <returns>The repository mock for chaining.</returns>
    public static Mock<TRepository> SetupFindAsync<TRepository, TEntity, TId>(
        this Mock<TRepository> repository,
        TId id,
        TEntity? entity)
        where TRepository : class, IReadRepository<TEntity, TId>
        where TEntity : BaseEntity
        where TId : struct
    {
        repository
            .Setup(x => x.FindAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        return repository;
    }
}