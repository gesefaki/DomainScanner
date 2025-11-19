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
        var domains = _domainService.GetAll();
        return Ok(domains);
    }

    [HttpGet("/api/domains/{id}")]
    public ActionResult<Domain?> Get(int id)
    {
        var domain = _domainService.Get(id);
        if (domain == null)
            return NotFound();
            
        return Ok(domain);
    }

    [HttpPost("/api/domains")]
    public ActionResult Add(Domain domain)
    {
        _domainService.Add(domain);
        return CreatedAtAction(nameof(Get), new { id = domain.Id }, domain);
    }

    [HttpDelete("/api/domains/{id}")]
    public ActionResult Remove(int id)
    {
        var domain = _domainService.Get(id);
        if (domain == null)
            return NotFound();

        _domainService.Remove(id);

        return NoContent();
    }

    [HttpPut("/api/domains/{id}")]
    public ActionResult Update(int id, Domain domain)
    {
        if (id != domain.Id)
            return BadRequest();

        var existingDomain = _domainService.Get(id);
        if (existingDomain == null)
            return NotFound();

        _domainService.Update(domain);

        return NoContent();
    }

    [HttpGet("/api/domains/{id}/health")]
    public ActionResult<bool> CheckHealth(int id)
    {

        var domain = _domainService.Get(id);
        if (domain == null)
            return NotFound();

        var isAvailable = _domainService.CheckHealth(id);

        return Ok(isAvailable);
    }
}
