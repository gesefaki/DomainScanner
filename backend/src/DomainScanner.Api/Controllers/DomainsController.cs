using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Api.Mapping;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Domains.Commands.CreateDomain;
using DomainScanner.Application.Domains.Commands.DeleteDomain;
using DomainScanner.Application.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.Domains.Commands.UpdateDomain;
using DomainScanner.Application.Domains.Queries.GetAllDomains;
using DomainScanner.Application.Domains.Queries.GetDomainById;
using DomainScanner.Application.Domains.Queries.GetHttpResponse;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

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
    public async Task<ActionResult<IEnumerable<DomainResponseDto>>> GetAll(CancellationToken ct = default)
    {
        var query = new GetAllDomainsQuery();
        var domains = await _mediator.Send(query, ct);
        var response = domains.Select(DomainsMapper.DomainToDomainResponseDto);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DomainResponseDto>> Get(Guid id, CancellationToken ct = default)
    {
        var query = new GetDomainByIdQuery(id);
        var domain = await _mediator.Send(query, ct);
        if (domain is null)
            return NotFound();
        
        var response = DomainsMapper.DomainToDomainResponseDto(domain);
        return Ok(response);
    }

    [HttpGet("http/check/{id:guid}")]
    public async Task<ActionResult<DomainResponseDto>> GetHttpCheck(Guid id, CancellationToken ct = default)
    {
        var domain =  await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (domain is null)
            throw new UserNotFoundException(nameof(User), id);

        var query = new GetHttpResponseQuery(domain);
        var response = await _mediator.Send(query, ct);
        return Ok(DomainsMapper.HttpResponseToHttpResponseDto(domain.Address, response));
    }

    [HttpPut("put/{id::guid}")]
    public async Task<ActionResult> Update([FromBody] UpdateDomainDto dto, Guid id, CancellationToken ct = default)
    {
        var existingDomain = await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (existingDomain is null)
            return NotFound();

        var updatedDomain = new DomainEntity()
        {
            Id = existingDomain.Id,
            Address = dto.Address,
            IsAvailable = dto.IsAvailable
        };
        
        var cmd = new UpdateDomainCommand(id,  updatedDomain);
        await _mediator.Send(cmd, ct);
        
        return CreatedAtAction(nameof(Get), new { id = cmd.Id }, cmd);
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create([FromBody] CreateDomainDto dto, CancellationToken ct = default)
    {
        var domain = DomainsMapper.CreateDomainDtoToUser(dto);
        var cmd = new CreateDomainCommand(domain);
        await _mediator.Send(cmd, ct);
        
        return CreatedAtAction(nameof(Get), new { id = cmd.Domain.Id }, cmd);
    }

    [HttpPost("http/check-and-save/{id:guid}")]
    public async Task<ActionResult> SendAndSave(Guid id, CancellationToken ct = default)
    {
        var domain = await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (domain is null)
            throw new UserNotFoundException(nameof(User), id);

        var cmd = new HttpSendAndSaveCommand(domain);
        await _mediator.Send(cmd, ct);
        return CreatedAtAction(nameof(Get), new { id = cmd.Domain.Id });
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var cmd = new DeleteDomainCommand(id);
        await _mediator.Send(cmd, ct);
        
        return NoContent();
    }
}