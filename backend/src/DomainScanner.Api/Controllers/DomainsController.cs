using DomainScanner.Api.DTOs.Domains;
using DomainScanner.Api.Mapping;
using DomainScanner.Application.Abstractions.Mediator;
using DomainScanner.Application.Domains.Commands.CreateDomain;
using DomainScanner.Application.Domains.Commands.DeleteDomain;
using DomainScanner.Application.Domains.Commands.HttpSendAndSave;
using DomainScanner.Application.Domains.Commands.UpdateDomain;
using DomainScanner.Application.Domains.Queries.GetAllDomains;
using DomainScanner.Application.Domains.Queries.GetAllDomainsByUser;
using DomainScanner.Application.Domains.Queries.GetDomainById;
using DomainScanner.Application.Domains.Queries.GetHttpDetails;
using DomainScanner.Application.Domains.Queries.GetHttpResponse;
using DomainScanner.Application.Exceptions;
using DomainScanner.Domain.Entities;
using FluentValidation;
using FluentValidation.Results;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class DomainsController : Controller
{
    private readonly IMediator _mediator;
    private readonly IValidator<DomainEntity> _validator;
    private readonly ILogger<DomainsController> _logger;

    public DomainsController(IMediator mediator, IValidator<DomainEntity> validator,
        ILogger<DomainsController> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
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

    [HttpGet("u/{id:guid}")]
    public async Task<ActionResult<IEnumerable<DomainResponseDto>>> GetUserDomains(Guid id,
        CancellationToken ct = default)
    {
        var query = new GetAllDomainsByUserQuery(id);
        var domains = await _mediator.Send(query, ct);
        var response = domains.Select(DomainsMapper.DomainToDomainResponseDto);
        return Ok(response);
    }

    [HttpGet("http/check/{id:guid}")]
    public async Task<ActionResult<DomainResponseDto>> GetHttpCheck(Guid id, CancellationToken ct = default)
    {
        var domain = await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (domain is null)
            throw new DomainNotFoundException(id);
        
        var query = new GetHttpResponseQuery(domain);
        var response = await _mediator.Send(query, ct);
        return Ok(DomainsMapper.HttpResponseToHttpResponseDto(domain.Address, response));
    }

    [HttpGet("http/check-with-details/{id:guid}")]
    public async Task<ActionResult<HttpResponseDetailsDto>> GetHttpCheckWithDetails(Guid id, CancellationToken ct = default)
    {
        var domain = await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (domain is null)
            throw new UserNotFoundException(id);

        var query = new GetHttpDetailsQuery(id);
        var result =  await _mediator.Send(query, ct);
        return Ok(DomainsMapper.HttpDetailsToDto(result));
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
        ValidationResult validationResult = await _validator.ValidateAsync(
            DomainsMapper.CreateDomainDtoToDomain(dto), ct);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation error.");
            throw new BadRequestException(validationResult.Errors.ToString()!);
        }

        var domain = DomainsMapper.CreateDomainDtoToDomain(dto);
        var cmd = new CreateDomainCommand(domain);
        await _mediator.Send(cmd, ct);
        
        return CreatedAtAction(nameof(Get), new { id = cmd.Domain.Id }, cmd);
    }

    [HttpPost("send-and-save/{id::guid}")]
    public async Task<ActionResult<HttpResponseDto>> SendAndSave(Guid id, CancellationToken ct = default)
    {
        var domain = await _mediator.Send(new GetDomainByIdQuery(id), ct);
        if (domain is null)
            throw new DomainNotFoundException(id);

        var cmd = new HttpSendAndSaveCommand(id);
        var check = await _mediator.Send(cmd, ct);

        return Ok(DomainResultsMapper.CheckToResponseDto(check));
    }

    [HttpDelete("delete/{id:guid}")]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        var cmd = new DeleteDomainCommand(id);
        await _mediator.Send(cmd, ct);
        
        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpPost("hangfire/cleanup")]
    public IActionResult CleanupHangfireJobs()
    {
        using var connection = JobStorage.Current.GetConnection();
    
        // Удалить все recurring jobs
        var recurringJobs = connection.GetRecurringJobs();
        foreach (var job in recurringJobs)
        {
            RecurringJob.RemoveIfExists(job.Id);
            _logger.LogInformation("Removed job: {JobId}", job.Id);
        }
    
        // Перезапустить Worker, чтобы он создал задачи заново
        return Ok($"Removed {recurringJobs.Count()} jobs. Restart worker to recreate them.");
    }
}