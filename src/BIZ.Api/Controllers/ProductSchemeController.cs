using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductSchemeController : ControllerBase
{
    private readonly IProductSchemeService _service;

    public ProductSchemeController(IProductSchemeService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductSchemeDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductSchemeDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductSchemeDto>> Create(
        ProductSchemeDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductSchemeDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}