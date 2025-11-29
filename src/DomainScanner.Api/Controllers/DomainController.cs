using DomainScanner.Core.Interfaces;
using DomainScanner.Core.DTO;
using DomainScanner.Core.Mappers;
using Microsoft.AspNetCore.Mvc;

namespace DomainScanner.Api.Controllers;

[Route("api/domains/v1")]
public class DomainController(IDomainService domainService) : ControllerBase
{
    private readonly IDomainService _service = domainService;
    
    // Получение всех доменов
    [HttpGet("")]
    public ActionResult<List<ResponseDomainDto>> GetAll()
    {
        var domains = _service.GetAll();
        var result = domains.Select(d => d.ToResponse()).ToList(); 
        return Ok(result);
    }

    // Получение домена по ID
    [HttpGet("{id:int}")]
    public ActionResult<ResponseDomainDto?> GetById(int id)
    {
        var domain = _service.GetById(id);
        if (domain == null)
            return NotFound();
            
        var result = domain.ToResponse();
        return Ok(result);
    }

    // Проверка состояния домена
    [HttpGet("{id:int}/health")]
    public async Task<ActionResult<bool>> CheckHealth(int id)
    {
        var domain = _service.GetById(id);
        if (domain == null)
            return NotFound();
        
        var result = await _service.CheckHealthAsync(id);
        return Ok(result);
    }

    // Добавление домена
    [HttpPost("")]
    public ActionResult<ResponseDomainDto> Add([FromBody] CreateDomainDto dto)
    {
        var domain = dto.ToDomain();
        _service.Add(domain);

        return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
    }

    // Обновление домена
    [HttpPut("{id:int}")]
    public ActionResult<ResponseDomainDto?> Update(int id, [FromBody] UpdateDomainDto dto)
    {
        if(!_service.IsExistsById(id))
            return NotFound();
        
        var domain = dto.ToDomain(id);
        
        if(id != domain.Id)
            return BadRequest();
        
        _service.Update(id, domain);
        return CreatedAtAction(nameof(GetById), new { id = domain.Id }, dto);
    }

    // Удаление домена
    [HttpDelete("{id:int}")]
    public ActionResult Delete(int id)
    {
        _service.Remove(id);
        return NoContent();
    }
}
