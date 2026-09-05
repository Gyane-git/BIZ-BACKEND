using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransactionController : ControllerBase
{
    private readonly IStockTransactionService _service;

    public StockTransactionController(
        IStockTransactionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<IEnumerable<StockTransactionDto>>>
        GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockTransactionDto>>
        GetById(int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("product/{productId}")]
    public async Task<
        ActionResult<IEnumerable<StockTransactionDto>>>
        GetByProduct(int productId)
    {
        return Ok(
            await _service.GetByProductAsync(productId));
    }

    [HttpPost]
    public async Task<ActionResult<StockTransactionDto>>
        Create(StockTransactionDto dto)
    {
        try
        {
            var result =
                await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result =
                await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}
