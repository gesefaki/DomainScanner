using AutoMapper;
using DomainScanner.Application.Handlers.Users.Commands.RegisterUser;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.DTOs.HTTPs.Responses;
using DomainScanner.Contracts.DTOs.Users.Responses;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.Models;

namespace DomainScanner.Application.Mapping;

/// <summary>
/// AutoMapper profile that defines all mapping configurations between domain entities and DTOs.
/// Provides centralized mapping rules for the entire application.
/// </summary>
public class MappingProfile : Profile
{
    /// <summary>
    /// Configures all AutoMapper mappings for the Domain Scanner application.
    /// </summary>
    public MappingProfile()
    {
        // HTTPs
        // Entity -> DTO
        CreateMap<DomainCheckResult, HttpResponse>()
            .ConstructUsing((src, context) => new HttpResponse(
                src.Address,
                src.StatusCode,
                src.StatusCode is >= 200 and <= 299,
                src.CreatedAt
            ));
        
        CreateMap<HttpResponseObject, HttpResponse>()
            .ConstructUsing((src, context) => new HttpResponse(
                src.Address,
                src.StatusCode,
                src.IsSuccess,
                src.CreatedAt
            ));
        
        // Domains
        // Entity -> DTO
        CreateMap<DomainEntity, DomainResponse>()
            .ConstructUsing((src, context) => new DomainResponse(
                src.Id,
                src.Address,
                src.IsActive,
                src.UserId,
                context.Mapper.Map<HttpResponse[]>(src.CheckResults ?? new List<DomainCheckResult>())));
        
        
        // Users
        // Entity -> DTO
        CreateMap<User, UserResponse>()
            .ConstructUsing((src, context) => new UserResponse(
                src.Id,
                src.Username,
                src.Email,
                src.IsActive,
                context.Mapper.Map<DomainResponse[]>(src.Domains ?? new List<DomainEntity>())));
        
        // Command -> Entity
        CreateMap<RegisterUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        
    }
}