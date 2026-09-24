using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Application.Mapping;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;

/// <summary>Verifies domain ownership, persists one HTTP check, and maps its public response.</summary>
public sealed class HttpSendAndSaveOwnedCommandHandler
    : IRequestHandler<HttpSendAndSaveOwnedCommand, DomainCheckResponse>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IDomainCheckExecutor _executor;

    /// <summary>Creates the command handler.</summary>
    public HttpSendAndSaveOwnedCommandHandler(
        IOwnedDomainProvider ownedDomains,
        IDomainCheckExecutor executor)
    {
        _ownedDomains = ownedDomains;
        _executor = executor;
    }

    /// <inheritdoc />
    public async Task<DomainCheckResponse> Handle(
        HttpSendAndSaveOwnedCommand request,
        CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(request.Id, ct);
        var check = await _executor.ExecuteAndSaveAsync(domain, ct);
        return DomainResponseMapping.ToCheck(check);
    }
}
