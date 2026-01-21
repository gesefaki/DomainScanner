using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Domain.Entities;

namespace DomainScanner.Api.Mapping;

public static class DomainResultsMapper
{
    public static HttpResponseDto CheckToResponseDto(DomainCheckResult check)
    {
        return new HttpResponseDto(
            Address: check.Address,
            StatusCode: check.StatusCode,
            IsSuccess: check.IsAvailable,
            CreateAt: DateTime.UtcNow
        );
    }
}