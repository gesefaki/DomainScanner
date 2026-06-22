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
using DomainScanner.Contracts.DTOs.HTTPs.Responses;
using Hangfire;
using Hangfire.Storage;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DomainsController : Controller
{
    private readonly IMediator _mediator;

    public DomainsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DomainResponse>>> GetAll(CancellationToken ct)
    {
        var query = new GetAllDomainsQuery();
        var domains = await _mediator.Send(query, ct);
        return Ok(domains);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Get(Guid id, CancellationToken ct)
    {
        var query = new GetDomainByIdQuery(id);
        var domain = await _mediator.Send(query, ct);
        return Ok(domain);
    }

    [HttpGet("http/check/{id:guid}")]
    public async Task<ActionResult<DomainResponse>> GetHttpCheck(Guid id, CancellationToken ct)
    {
        var query = new GetHttpResponseQuery(id);
        var response = await _mediator.Send(query, ct);
        return Ok(response);
    }

    [HttpGet("http/check-with-details/{id:guid}")]
    public async Task<ActionResult<HttpResponseDetails>> GetHttpCheckWithDetails(Guid id, CancellationToken ct)
    {
        var query = new GetHttpDetailsQuery(id);
        var result =  await _mediator.Send(query, ct);
        return Ok(result);
    }

    [HttpPut("{id::guid}")]
    public async Task<ActionResult> Update(Guid id, 
        [FromBody] UpdateDomainRequest request, 
        CancellationToken ct)
    {
        var cmd = new UpdateDomainCommand(id, request);
        var result = await _mediator.Send(cmd, ct);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDomainRequest request, CancellationToken ct)
    {
        var cmd = new CreateDomainCommand(request);
        var domain = await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get), new { id = domain.Id }, domain);
    }

    [HttpPost("send-and-save/{id::guid}")]
    public async Task<ActionResult<DomainResponse>> SendAndSave(Guid id, CancellationToken ct = default)
    {
        var cmd = new HttpSendAndSaveCommand(id);
        var check = await _mediator.Send(cmd, ct);
        return Ok(check);
    }
    
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(DeleteDomainRequest request, CancellationToken ct)
    {
        var cmd = new DeleteDomainCommand(request);
        await _mediator.Send(cmd, ct);
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