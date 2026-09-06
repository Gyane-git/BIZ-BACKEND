using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransferLineController
    : ControllerBase
{
    private readonly IStockTransferLineService _service;

    public StockTransferLineController(
        IStockTransferLineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("transfer/{stockTransferId:int}")]
    public async Task<IActionResult> GetByTransfer(
        int stockTransferId)
    {
        return Ok(
            await _service.GetByTransferAsync(
                stockTransferId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] StockTransferLineDto dto)
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] StockTransferLineDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message =
                    "Stock transfer line updated successfully."
            });
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result =
                await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message =
                    "Stock transfer line deleted successfully."
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
}