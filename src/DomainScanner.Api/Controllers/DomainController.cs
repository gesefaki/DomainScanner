using DomainScanner.Api.DTOs;
using DomainScanner.Infrastructure.Models;
using DomainScanner.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

public class DomainController(IDomainService domainService) : ControllerBase
{
    private readonly IDomainService _domainService = domainService;

    [HttpGet("/api/domains")]
    public ActionResult<List<Domain>> GetAll()
    {
        var domains = _domainService.GetAllDto();
        return Ok(domains);
    }

    [HttpGet("/api/domains/{id}")]
    public ActionResult<Domain?> Get(int id)
    {
        var domain = _domainService.GetDto(id);
        if (domain == null)
            return NotFound();
            
        return Ok(domain);
    }

    [HttpPost("/api/domains")]
    public ActionResult Add(CreateDomainDto dto)
    {
        _domainService.Add(dto);
        return CreatedAtAction(
            nameof(Get),
            new { id = dto.Id },
            dto
        );
    }

    [HttpDelete("/api/domains/{id}")]
    public ActionResult Remove(int id)
    {
        var domain = _domainService.GetDto(id);
        if (domain == null)
            return NotFound();

        _domainService.Remove(id);

        return NoContent();
    }

    [HttpPut("/api/domains/{id}")]
    public ActionResult Update(int id, UpdateDomainDto dto)
    {
        if (id != dto.Id)
            return BadRequest();

        var existingDomain = _domainService.GetDto(id);
        if (existingDomain == null)
            return NotFound();

        _domainService.UpdateDto(dto);

        return NoContent();
    }

    [HttpGet("/api/domains/{id}/health")]
    public ActionResult CheckHealth(int id)
    {

        var dto = _domainService.GetDto(id);
        if (dto == null)
            return NotFound();

        _domainService.UpdateHealthStatus(dto.Id);
        return Ok(dto.IsAvailable);
    }
}
