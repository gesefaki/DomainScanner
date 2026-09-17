using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Contracts.Exceptions.Common;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Services.Auth;

/// <summary>
/// Retrieves domains owned by the current authenticated user and prevents access to foreign domains.
/// </summary>
public sealed class OwnedDomainProvider : IOwnedDomainProvider
{
    private readonly IRepository<DomainEntity, Guid> _repository;
    private readonly ICurrentUser _currentUser;

    public OwnedDomainProvider(
        IRepository<DomainEntity, Guid> repository,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    /// <exception cref="NonAuthenticatedException">Thrown when the current user is not authenticated.</exception>
    /// <exception cref="DomainNotFoundException">
    /// Thrown when the domain does not exist or is not owned by the current user.
    /// </exception>
    public async Task<DomainEntity> GetRequiredAsync(Guid domainId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new NonAuthenticatedException();
        }

        var currentUserId = _currentUser.Id;

        var domain = await _repository.GetAsync(
            entity =>
                entity.Id == domainId &&
                entity.UserId == currentUserId,
            ct
        );

        if (domain is null)
        {
            throw new DomainNotFoundException(domainId);
        }

        return domain;
    }
}
