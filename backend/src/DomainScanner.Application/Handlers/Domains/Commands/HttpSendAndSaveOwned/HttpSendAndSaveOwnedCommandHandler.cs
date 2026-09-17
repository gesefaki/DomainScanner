using DomainScanner.Application.Abstractions.Auth;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;

/// <summary>
/// Handles <see cref="HttpSendAndSaveOwnedCommand"/> by verifying domain ownership
/// and delegating the HTTP check to <see cref="IDomainCheckExecutor"/>.
/// </summary>
public sealed class HttpSendAndSaveOwnedCommandHandler
    : IRequestHandler<HttpSendAndSaveOwnedCommand, DomainCheckResult>
{
    private readonly IOwnedDomainProvider _ownedDomains;
    private readonly IDomainCheckExecutor _executor;

    public HttpSendAndSaveOwnedCommandHandler(
        IOwnedDomainProvider ownedDomains,
        IDomainCheckExecutor executor
    )
    {
        _ownedDomains = ownedDomains;
        _executor = executor;
    }

    /// <inheritdoc />
    public async Task<DomainCheckResult> Handle(HttpSendAndSaveOwnedCommand request,
        CancellationToken ct)
    {
        var domain = await _ownedDomains.GetRequiredAsync(
            request.Id,
            ct);

        return await _executor.ExecuteAndSaveAsync(
            domain,
            ct);
    }
}
