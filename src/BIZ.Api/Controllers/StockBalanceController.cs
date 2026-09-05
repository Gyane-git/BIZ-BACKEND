using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockBalanceController : ControllerBase
{
    private readonly IStockBalanceService _service;

    public StockBalanceController(
        IStockBalanceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<IEnumerable<StockBalanceDto>>>
        GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockBalanceDto>>
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
        ActionResult<IEnumerable<StockBalanceDto>>>
        GetByProduct(int productId)
    {
        return Ok(
            await _service.GetByProductAsync(productId));
    }

    [HttpGet("warehouse/{warehouseId}")]
    public async Task<
        ActionResult<IEnumerable<StockBalanceDto>>>
        GetByWarehouse(int warehouseId)
    {
        return Ok(
            await _service.GetByWarehouseAsync(warehouseId));
    }

    [HttpPost]
    public async Task<ActionResult<StockBalanceDto>>
        Create(StockBalanceDto dto)
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
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        StockBalanceDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (ArgumentException ex)
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
        var result =
            await _service.DeleteAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}