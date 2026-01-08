using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Domain.Entities;
using DomainScanner.Domain.ValueObjects;

namespace DomainScanner.Api.Mapping;

public static class DomainsMapper
{
    public static DomainResponseDto DomainToDomainResponseDto(DomainEntity domain)
    {
        return new DomainResponseDto(
            domain.Id,
            domain.Address,
            domain.IsAvailable,
            domain.UserId,
            domain.CheckResults.Select(CheckResultToHttpResponseDto).ToArray());
    }

    public static DomainEntity CreateDomainDtoToUser(CreateDomainDto dto)
    {
        return new DomainEntity
        {
            Id = Guid.NewGuid(),
            Address = dto.Address,
            IsAvailable = null,
            UpdatedAt = null,
            CheckResults = [],
            UserId = dto.UserId,
            
        };
    }

    public static HttpResponseDto HttpResponseToHttpResponseDto(string address, 
        HttpResponseObject response)
    {
        return new HttpResponseDto(
            address,
            response.StatusCode,
            response.IsSuccess,
            response.CreatedAt
        );
    }

    public static HttpResponseDto CheckResultToHttpResponseDto(DomainCheckResult checkResult)
    {
        return new HttpResponseDto(
            checkResult.Address,
            checkResult.StatusCode,
            checkResult.IsAvailable,
            checkResult.CreatedAt);
    }

    public static HttpResponseDetailsDto HttpDetailsToDto(HttpResponseDetails details)
    {
        return new HttpResponseDetailsDto()
        {
            Address = details.Address,
            StatusCode = details.StatusCode,
            IsSuccess = details.IsSuccess,
            ResponseTime = details.ResponseTime,
            ReasonPhrase = details.ReasonPhrase,
            ContentType = details.ContentType,
            ContentLength = details.ContentLength,
            ErrorMessage = details.ErrorMessage,
            Version = details.Version,
            Tls = details.Tls
        };
    }
    
}