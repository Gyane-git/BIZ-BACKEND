using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    // GET: api/Unit
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var units = await _unitService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = units
        });
    }

    // GET: api/Unit/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);

        if (unit == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = unit
        });
    }

    // POST: api/Unit
    [HttpPost]
    public async Task<IActionResult> Create(UnitDto dto)
    {
        try
        {
            var unit = await _unitService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = unit.Id },
                new
                {
                    success = true,
                    message = "Unit created successfully.",
                    data = unit
                });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // PUT: api/Unit/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        UnitDto dto)
    {
        try
        {
            var updated = await _unitService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Unit not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Unit updated successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // DELETE: api/Unit/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _unitService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Unit deleted successfully."
        });
    }
}