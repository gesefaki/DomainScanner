using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

/// <summary>Executes an internal scheduled check after revalidating monitoring and owner eligibility.</summary>
/// <remarks>Does not use the current HTTP user; user-initiated scans use a separate owned-domain handler.</remarks>
public sealed class HttpSendAndSaveCommandHandler
    : IRequestHandler<
        HttpSendAndSaveCommand,
        DomainCheckResult>
{
    private readonly IDomainScanRepository _domains;
    private readonly IDomainCheckExecutor _executor;

    public HttpSendAndSaveCommandHandler(
        IDomainScanRepository domains,
        IDomainCheckExecutor executor)
    {
        _domains = domains;
        _executor = executor;
    }

    /// <summary>Loads an eligible domain and delegates scanning and persistence to the executor.</summary>
    /// <param name="request">The domain to check.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The saved check result.</returns>
    /// <exception cref="DomainNotFoundException">The domain is missing or no longer eligible for monitoring.</exception>
    public async Task<DomainCheckResult> Handle(
        HttpSendAndSaveCommand request,
        CancellationToken ct)
    {
        var domain = await _domains.GetForScanAsync(
            request.Id,
            ct);

        if (domain is null)
        {
            throw new DomainNotFoundException(
                request.Id);
        }

        return await _executor.ExecuteAndSaveAsync(
            domain,
            ct);
    }
}
