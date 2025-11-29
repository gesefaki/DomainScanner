using DomainScanner.Core.Interfaces;
using DomainScanner.Core.DTO;
using DomainScanner.Core.Mappers;
using DomainScanner.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Route("api/domains/v1")]
public class DomainController(IDomainService domainService) : ControllerBase
{
    private readonly IDomainService _service = domainService;
    
    // Получение всех доменов
    [HttpGet("")]
    public async Task<ActionResult<List<ResponseDomainDto>>> GetAllAsync()
    {
        var domains = await _service.GetAllAsync();
        var result = domains.Select(d => d.ToResponse()).ToList(); 
        return Ok(result);
    }

    // Получение домена по ID
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseDomainDto?>> GetById(int id)
    {
        var domain = await _service.GetByIdAsync(id);
        if (domain == null)
            return NotFound();
            
        var result = domain.ToResponse();
        return Ok(result);
    }

    // Проверка состояния домена
    [HttpGet("{id:int}/health")]
    public async Task<ActionResult<bool>> CheckHealthAsync(int id)
    {
        var domain = await _service.GetByIdAsync(id);
        if (domain == null)
            return NotFound();
        
        var result = await _service.CheckHealthAsync(id);
        return Ok(result);
    }

    [HttpGet("{id:int}/health/details")]
    public async Task<ActionResult<DomainHealth?>> GetHealthAsync(int id)
    {
        var domain = await _service.GetByIdAsync(id);
        if (domain == null)
            return NotFound();
        
        var health = await _service.GetHealthAsync(id);
        return Ok(health);
    }

    // Добавление домена
    [HttpPost("")]
    public async Task<ActionResult<ResponseDomainDto>> AddAsync([FromBody] CreateDomainDto dto)
    {
        var domain = dto.ToDomain();
        await _service.AddAsync(domain);

        return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
    }

    // Обновление домена
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ResponseDomainDto?>> UpdateAsync(int id, [FromBody] UpdateDomainDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();
        
        var domain = dto.ToDomain(id);
        
        if(id != domain.Id)
            return BadRequest();
        
        await _service.UpdateAsync(id, domain);
        return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
    }

    // Удаление домена
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.RemoveAsync(id);
        return NoContent();
    }
}
