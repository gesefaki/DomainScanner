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
    private readonly ISender _sender;

    public DomainsController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<DomainResponse>>> GetAll(CancellationToken ct)
    {
        var domains = await _sender.Send(new GetAllDomainsQuery(), ct);
        return Ok(domains);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainResponse>> Get(Guid id, CancellationToken ct)
    {
        var domain = _sender.Send(new GetDomainByIdQuery(id), ct);
        return Ok(domain);
    }

    [HttpGet("{id:guid}/http/check")]
    public async Task<ActionResult<DomainResponse>> GetHttpCheck(Guid id, CancellationToken ct)
    {
        var response = await _sender.Send(new GetHttpResponseQuery(id), ct);
        return Ok(response);
    }

    [HttpGet("{id:guid}/http/check-details")]
    public async Task<ActionResult<HttpResponseDetails>> GetHttpCheckWithDetails(Guid id, CancellationToken ct)
    {
        var result =  await _sender.Send(new GetHttpDetailsQuery(id), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, 
        [FromBody] UpdateDomainRequest request, 
        CancellationToken ct)
    {
        var result = await _sender.Send(new UpdateDomainCommand(id, request), ct);
        return Ok(result);
    }
    
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateDomainRequest request, CancellationToken ct)
    {
        var domain = await _sender.Send(new CreateDomainCommand(request), ct);
        return CreatedAtAction(nameof(Get), new { id = domain.Id }, domain);
    }

    [HttpPost("{id:guid}/send-save")]
    public async Task<ActionResult<DomainResponse>> SendAndSave(Guid id, CancellationToken ct)
    {
        var check = await _sender.Send(new HttpSendAndSaveCommand(id), ct);
        return Ok(check);
    }
    
    
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