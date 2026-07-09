using DomainScanner.Application.Handlers.Domains.Commands.CreateDomain;
using DomainScanner.Application.Handlers.Domains.Commands.DeleteDomain;
using DomainScanner.Application.Handlers.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.Handlers.Domains.Commands.UpdateDomain;
using DomainScanner.Application.Handlers.Domains.Queries.GetAllDomains;
using DomainScanner.Application.Handlers.Domains.Queries.GetDomainById;
using DomainScanner.Application.Handlers.Domains.Queries.GetHttpDetails;
using DomainScanner.Application.Handlers.Domains.Queries.GetHttpResponse;
using DomainScanner.Contracts.DTOs.Domains.Requests;
using DomainScanner.Contracts.DTOs.Domains.Responses;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using HttpResponse = DomainScanner.Contracts.DTOs.HTTPs.Responses.HttpResponse;
using HttpResponseDetails = DomainScanner.Contracts.DTOs.HTTPs.Responses.HttpResponseDetails;

namespace DomainScanner.Api.Controllers;

/// <summary>
/// REST API controller handles domains management operations.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class DomainsController : Controller
{
    private readonly ISender _sender;

    public DomainsController(ISender sender)
    {
        _sender = sender;
    }
    
    /// <summary>
    /// Retrieves all DomainEntity for authenticated user.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of <see cref="DomainResponse"/>. </returns>
    [HttpGet]
    public async Task<ActionResult<List<DomainResponse>>> GetAll(CancellationToken ct)
    {
        var domains = await _sender.Send(new GetAllDomainsQuery(), ct);
        return Ok(domains);
    }

    /// <summary>
    /// Retrieves single DomainEntity as <see cref="DomainResponse"/> for authenticated user. 
    /// </summary>
    /// <param name="id">Unique DomainEntity identifier.</param>
    /// <param name="ct">Cancellation toker.</param>
    /// <returns>Single <see cref="DomainResponse"/></returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Get(Guid id, CancellationToken ct)
    {
        var domain = await _sender.Send(new GetDomainByIdQuery(id), ct);
        return Ok(domain);
    }

    /// <summary>
    /// Permorms a basic HTTP health check.
    /// </summary>
    /// <param name="id">Unique DomainEntity identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="HttpResponse"/></returns>
    [HttpGet("{id:guid}/http/check")]
    public async Task<ActionResult<HttpResponse>> GetHttpCheck(Guid id, CancellationToken ct)
    {
        var response = await _sender.Send(new GetHttpResponseQuery(id), ct);
        return Ok(response);
    }
    
    /// <summary>
    /// Performs a detailed HTTP health check.
    /// </summary>
    /// <param name="id">Unique DomainEntity identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="HttpResponseDetails"/>.</returns>
    [HttpGet("{id:guid}/http/check-details")]
    public async Task<ActionResult<HttpResponseDetails>> GetHttpCheckWithDetails(Guid id, CancellationToken ct)
    {
        var result =  await _sender.Send(new GetHttpDetailsQuery(id), ct);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing domain.
    /// </summary>
    /// <param name="id">Unique identifier of the DomainEntity to update.</param>
    /// <param name="request">Request containing new data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="DomainResponse"/>.</returns> 
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Update(Guid id, 
        [FromBody] UpdateDomainRequest request, 
        CancellationToken ct)
    {
        var result = await _sender.Send(new UpdateDomainCommand(id, request), ct);
        return Ok(result);
    }
    
    /// <summary>
    /// Creates a new DomainEntity.
    /// </summary>
    /// <param name="request">Creation request.</param>
    /// <param name="ct">Cancellation Token.</param>
    [HttpPost]
    public async Task<ActionResult<DomainResponse>> Create([FromBody] CreateDomainRequest request, CancellationToken ct)
    {
        var domain = await _sender.Send(new CreateDomainCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id = domain.Id }, domain);
    }

    /// <summary>
    /// Sends an HTTP request to the domain and saves the result in database.
    /// </summary>
    /// <param name="id">DomainEntity unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Single <see cref="DomainResponse"/>.</returns>
    [HttpPost("{id:guid}/send-save")]
    public async Task<ActionResult<DomainResponse>> SendAndSave(Guid id, CancellationToken ct)
    {
        var check = await _sender.Send(new HttpSendAndSaveCommand(id), ct);
        return Ok(check);
    }
    
    /// <summary>
    /// Deletes DomainEntity from database. Not soft delete.
    /// </summary>
    /// <param name="id">DomainEntity unique identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _sender.Send(new DeleteDomainCommand(id), ct);
        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpPost("hangfire/cleanup")]
    public IActionResult CleanupHangfireJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
        
        var recurringJobs = connection.GetRecurringJobs();
        foreach (var job in recurringJobs)
        {
            RecurringJob.RemoveIfExists(job.Id);
        }
        
        return Ok($"Removed {recurringJobs.Count()} jobs. Restart worker to recreate them.");
    }
}