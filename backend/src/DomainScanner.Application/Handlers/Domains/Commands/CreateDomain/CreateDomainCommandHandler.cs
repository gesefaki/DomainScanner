using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

/// <summary>
/// Handles <see cref="CreateDomainCommand"/>. Has a <see cref="CreateDomainCommandValidator"/> must be passed. 
/// </summary>
public class CreateDomainCommandHandler : IRequestHandler<CreateDomainCommand, DomainResponse>
{
    private readonly IReadRepository<User, Guid> _usersReadRepository;
    private readonly IRepository<DomainEntity, Guid> _domainsRepository;
    private readonly IMapper _mapper;

    public CreateDomainCommandHandler(IReadRepository<User, Guid> usersReadRepository, 
        IRepository<DomainEntity, Guid> domainsRepository,
        IMapper mapper)
    {
        _usersReadRepository = usersReadRepository;
        _domainsRepository = domainsRepository;
        _mapper = mapper;
    }
    
    /// <inheritdoc />
    public async Task<DomainResponse> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        // find user
        var user = await _usersReadRepository.FindAsync(request.Request.UserId, ct);

        // throw if user is null
        if (user is null)
        {
            throw new UserNotFoundException(request.Request.UserId);
        }

        // create new domainEntity
        var domain = new DomainEntity
        {
            Address = request.Request.Address!,
            UserId = request.Request.UserId
        };

        // add domain in db
        var createdDomain = await _domainsRepository.CreateAsync(domain, ct);
        
        // link domain to user who created
        user.Domains.Add(createdDomain);
        
        return _mapper.Map<DomainResponse>(domain);
    }
}