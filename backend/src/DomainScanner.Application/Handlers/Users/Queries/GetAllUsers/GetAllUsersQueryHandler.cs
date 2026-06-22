using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
{
    private readonly IReadRepository<User> _repository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IReadRepository<User> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var result = await _repository.GetAllAsync(ct);
        return result.Select(_mapper.Map<UserResponse>);
    }
}