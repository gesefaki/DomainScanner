using AutoMapper;
using DomainScanner.Application.Abstractions.Persistence.Common;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Contracts.Exceptions.Users;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserResponse>
{
    private readonly IReadRepository<User> _repository;
    private readonly IMapper _mapper;
    
    public GetUserByIdQueryHandler(IReadRepository<User> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;

    }
    
    public async Task<UserResponse> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var result = await _repository.FindAsync(request.Request.Id, ct);
        if (result is null)
        {
            throw new UserNotFoundException(request.Request.Id);
        }

        return _mapper.Map<UserResponse>(result);
    }
}