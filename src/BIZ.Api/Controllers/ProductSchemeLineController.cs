using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductSchemeLineController : ControllerBase
{
    private readonly IProductSchemeLineService _service;

    public ProductSchemeLineController(
        IProductSchemeLineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductSchemeLineDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductSchemeLineDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("scheme/{productSchemeId:int}")]
    public async Task<ActionResult<IEnumerable<ProductSchemeLineDto>>>
        GetByScheme(int productSchemeId)
    {
        return Ok(await _service.GetBySchemeAsync(productSchemeId));
    }

    [HttpPost]
    public async Task<ActionResult<ProductSchemeLineDto>> Create(
        ProductSchemeLineDto dto)
    {
        try
        {
            return Ok(await _service.CreateAsync(dto));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductSchemeLineDto dto)
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