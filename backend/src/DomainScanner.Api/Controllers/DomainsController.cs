using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSaveOwned;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainCheckById;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainChecks;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using DomainScanner.Contracts.Options.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DomainScanner.Api.Controllers;

/// <summary>Manages owned domains and their persisted protocol-specific checks.</summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class DomainsController : ControllerBase
{
    private readonly ISender _sender;

    /// <summary>Creates the controller.</summary>
    public DomainsController(ISender sender) => _sender = sender;

    /// <summary>Gets an owned domain with its latest check summary.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Read)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Get(Guid id, CancellationToken ct) =>
        Ok(await _sender.Send(new GetDomainByIdQuery(id), ct));

    /// <summary>Gets retained checks of an owned domain, newest first.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Read)]
    [HttpGet("{id:guid}/checks")]
    public async Task<ActionResult<IReadOnlyList<DomainCheckResponse>>> GetChecks(
        Guid id, CancellationToken ct) =>
        Ok(await _sender.Send(new GetDomainChecksQuery(id), ct));

    /// <summary>Gets a single persisted check of an owned domain.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="checkId">Check identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Read)]
    [HttpGet("{id:guid}/checks/{checkId:guid}")]
    public async Task<ActionResult<DomainCheckResponse>> GetCheck(
        Guid id, Guid checkId, CancellationToken ct) =>
        Ok(await _sender.Send(new GetDomainCheckByIdQuery(id, checkId), ct));

    /// <summary>Creates a domain for the current user.</summary>
    /// <param name="request">Configured address.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Write)]
    [HttpPost]
    public async Task<ActionResult<DomainResponse>> Create(
        [FromBody] CreateDomainRequest request, CancellationToken ct)
    {
        var domain = await _sender.Send(new CreateDomainCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id = domain.Id }, domain);
    }

    /// <summary>Updates the address and monitoring setting of an owned domain.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="request">New domain settings.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Write)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Update(
        Guid id, [FromBody] UpdateDomainRequest request, CancellationToken ct) =>
        Ok(await _sender.Send(new UpdateDomainCommand(id, request), ct));

    /// <summary>Runs and persists an HTTP check of an owned domain.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Scan)]
    [HttpPost("{id:guid}/http/checks")]
    public async Task<ActionResult<DomainCheckResponse>> RunHttpCheck(
        Guid id, CancellationToken ct)
    {
        var check = await _sender.Send(new HttpSendAndSaveOwnedCommand(id), ct);
        return CreatedAtAction(nameof(GetCheck),
            new { id, checkId = check.Id }, check);
    }

    /// <summary>Deletes an owned domain and its retained checks.</summary>
    /// <param name="id">Domain identifier.</param>
    /// <param name="ct">Request cancellation token.</param>
    [EnableRateLimiting(RateLimitingSettings.Policies.Write)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteDomainCommand(id), ct);
        return NoContent();
    }
}
