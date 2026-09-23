using AutoMapper;
using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Contracts.Options.Domains;
using DomainScanner.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

/// <summary>
/// Creates a monitored domain for the current active user, subject to their domain quota.
/// </summary>
/// <remarks>
/// The quota counts all owned domains, including paused and unavailable ones.
/// Validation is performed by <see cref="CreateDomainCommandValidator"/> before handling.
/// </remarks>
public class CreateDomainCommandHandler : IRequestHandler<CreateDomainCommand, DomainResponse>
{
    private readonly IReadRepository<User, Guid> _usersReadRepository;
    private readonly IRepository<DomainEntity, Guid> _domainsRepository;
    private readonly IMapper _mapper;
    private readonly ICurrentUser _currentUser;
    private readonly DomainQuotaOptions _quota;

    public CreateDomainCommandHandler(IReadRepository<User, Guid> usersReadRepository, 
        IRepository<DomainEntity, Guid> domainsRepository,
        IMapper mapper,
        ICurrentUser currentUser,
        IOptions<DomainQuotaOptions> quota)
    {
        _usersReadRepository = usersReadRepository;
        _domainsRepository = domainsRepository;
        _mapper = mapper;
        _currentUser = currentUser;
        _quota = quota.Value;
    }
    
    /// <summary>Checks ownership quota and stages a new domain for persistence by the unit of work.</summary>
    /// <param name="request">The validated creation command.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The newly created domain with monitoring enabled.</returns>
    /// <exception cref="UserNotFoundException">The current user is missing or inactive.</exception>
    /// <exception cref="DomainQuotaExceededException">The user has reached their domain quota.</exception>
    public async Task<DomainResponse> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        var userId = _currentUser.Id;

        // find user
        var userExists =
            await _usersReadRepository.IsExistsByAttribute(
                user =>
                    user.Id == userId &&
                    user.IsActive,
                ct);

        if (!userExists)
        {
            throw new UserNotFoundException(userId);
        }

        var domainCount = await _domainsRepository.CountAsync(
            domain => domain.UserId == userId,
            ct);

        if (domainCount >= _quota.MaxDomainsPerUser)
        {
            throw new DomainQuotaExceededException(
                _quota.MaxDomainsPerUser);
        }

        // create new domainEntity
        var domain = new DomainEntity
        {
            Address = request.Request.Address!,
            UserId = userId,
        };

        // add domain in db
        await _domainsRepository.CreateAsync(domain, ct);

        return _mapper.Map<DomainResponse>(domain);
    }
}
