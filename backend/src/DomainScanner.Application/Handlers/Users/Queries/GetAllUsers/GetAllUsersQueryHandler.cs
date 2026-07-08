using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetAllUsers;

/// <summary>
/// Handles <see cref="GetAllUsersQuery"/>. 
/// </summary>
public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserResponse>>
{
    private readonly IReadRepository<User, Guid> _repository;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IReadRepository<User, Guid> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<UserResponse>> Handle(GetAllUsersQuery request, CancellationToken ct)
    {
        var result = await _repository.GetAllAsync(ct);
        return result.Select(_mapper.Map<UserResponse>);
    }
}