using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryChallanController : ControllerBase
{
    private readonly IDeliveryChallanService _service;

    public DeliveryChallanController(
        IDeliveryChallanService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _service.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        if (result == null)
        {
            return NotFound(new
            {
                message =
                    "Delivery challan not found."
            });
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] DeliveryChallanDto dto)
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] DeliveryChallanDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    message =
                        "Delivery challan not found."
                });
            }

            return Ok(new
            {
                message =
                    "Delivery challan updated successfully."
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
    public async Task<IActionResult> Delete(
        int id)
    {
        var result =
            await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                message =
                    "Delivery challan not found."
            });
        }

        return Ok(new
        {
            message =
                "Delivery challan deactivated successfully."
        });
    }
}