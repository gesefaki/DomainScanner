using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;

public class CreateDomainCommandHandler : IRequestHandler<CreateDomainCommand, DomainResponse>
{
    private readonly IReadRepository<User> _userReadRepository;
    private readonly IReadRepository<DomainEntity> _domainReadRepository;
    private readonly IWriteRepository<DomainEntity> _domainWriteRepository;
    private readonly IMapper _mapper;

    public CreateDomainCommandHandler(IReadRepository<User> userReadRepository, 
        IReadRepository<DomainEntity> domainReadRepository, 
        IWriteRepository<DomainEntity> domainWriteRepository, 
        IMapper mapper)
    {
        _userReadRepository = userReadRepository;
        _domainReadRepository = domainReadRepository;
        _domainWriteRepository = domainWriteRepository;
        _mapper = mapper;
    }
    
    public async Task<DomainResponse> Handle(CreateDomainCommand request, CancellationToken ct)
    {
        // find user
        var user = await _userReadRepository.FindAsync(request.Request.UserId, ct);

        // throw if user is null
        if (user is null)
        {
            throw new UserNotFoundException(request.Request.UserId);
        }

        // create new domainEntity
        var domain = new DomainEntity
        {
            Address = request.Request.Address
        };

        // add domain in db
        var createdDomain = await _domainWriteRepository.CreateAsync(domain, ct);
        
        // link domain to user who created
        user.Domains.Add(createdDomain);
        
        return _mapper.Map<DomainResponse>(domain);
    }
}