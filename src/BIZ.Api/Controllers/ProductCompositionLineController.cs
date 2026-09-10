using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCompositionLineController : ControllerBase
{
    private readonly IProductCompositionLineService _service;

    public ProductCompositionLineController(
        IProductCompositionLineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCompositionLineDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductCompositionLineDto>> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("composition/{productCompositionId:int}")]
    public async Task<ActionResult<IEnumerable<ProductCompositionLineDto>>>
        GetByComposition(int productCompositionId)
    {
        return Ok(
            await _service.GetByCompositionAsync(productCompositionId));
    }

    [HttpPost]
    public async Task<ActionResult<ProductCompositionLineDto>> Create(
        ProductCompositionLineDto dto)
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
        ProductCompositionLineDto dto)
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