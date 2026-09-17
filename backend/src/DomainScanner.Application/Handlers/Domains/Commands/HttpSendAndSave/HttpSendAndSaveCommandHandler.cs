using DomainScanner.Application.Abstractions.Persistence;
using DomainScanner.Application.Abstractions.Scanners;
using DomainScanner.Contracts.Exceptions.Domains;
using DomainScanner.Domain.Entities;
using MediatR;

namespace DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;

/// <summary>
/// Handles <see cref="HttpSendAndSaveCommand"/> for background processing by loading a domain
/// without applying HTTP user ownership checks and delegating the check to <see cref="IDomainCheckExecutor"/>.
/// </summary>
public sealed class HttpSendAndSaveCommandHandler
    : IRequestHandler<HttpSendAndSaveCommand, DomainCheckResult>
{
    private readonly IRepository<DomainEntity, Guid> _domainsRepository;
    private readonly IDomainCheckExecutor _executor;

    public HttpSendAndSaveCommandHandler(
        IRepository<DomainEntity, Guid> domainsRepository,
        IDomainCheckExecutor executor)
    {
        _domainsRepository = domainsRepository;
        _executor = executor;
    }

    /// <inheritdoc />
    public async Task<DomainCheckResult> Handle(
        HttpSendAndSaveCommand request,
        CancellationToken ct)
    {
        var domain = await _domainsRepository.FindAsync(request.Id, ct);

        if (domain is null)
        {
            throw new DomainNotFoundException(request.Id);
        }

        return await _executor.ExecuteAndSaveAsync(
            domain,
            ct);
    }
}
