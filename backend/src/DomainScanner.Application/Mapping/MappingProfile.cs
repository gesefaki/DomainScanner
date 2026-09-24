using AutoMapper;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Application.Mapping;

/// <summary>Maps entities to the protocol-neutral public domain and check contracts.</summary>
public sealed class MappingProfile : Profile
{
    /// <summary>Registers mappings used by API commands and queries.</summary>
    public MappingProfile()
    {
        CreateMap<DomainCheckResult, DomainCheckResponse>()
            .ConstructUsing((check, _) => DomainResponseMapping.ToCheck(check));

        CreateMap<DomainEntity, DomainResponse>()
            .ConstructUsing((domain, _) => DomainResponseMapping.ToDomain(domain));

        CreateMap<User, UserResponse>()
            .ConstructUsing((user, context) => new UserResponse(
                user.Id,
                user.Username,
                user.Email,
                user.IsActive,
                context.Mapper.Map<DomainResponse[]>(user.Domains)));

        CreateMap<RegisterUserCommand, User>()
            .ForMember(user => user.Id, options => options.Ignore())
            .ForMember(user => user.CreatedAt,
                options => options.MapFrom(_ => DateTime.UtcNow));
    }
}
