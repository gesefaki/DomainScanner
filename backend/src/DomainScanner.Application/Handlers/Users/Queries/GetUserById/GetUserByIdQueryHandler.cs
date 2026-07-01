using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IReadRepository<User, Guid> _repository;
    private readonly IMapper _mapper;
    
    public GetUserByIdQueryHandler(IReadRepository<User, Guid> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;

    }
    
    public async Task<UserResponse> Handle(GetUserByIdQuery query, CancellationToken ct)
    {
        var result = await _repository.FindAsync(query.Id, ct);
        if (result is null)
        {
            throw new UserNotFoundException(query.Id);
        }

        return _mapper.Map<UserResponse>(result);
    }
}