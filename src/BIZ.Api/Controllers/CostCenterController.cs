using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostCenterController : ControllerBase
{
    private readonly ICostCenterService _service;

    public CostCenterController(
        ICostCenterService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data == null)
        {
            return NotFound(new
            {
                success = false,
                message = "CostCenter not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(
        string code)
    {
        var data = await _service.GetByCodeAsync(code);

        if (data == null)
        {
            return NotFound(new
            {
                success = false,
                message = "CostCenter not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetByBranch(
        int branchId)
    {
        var data = await _service
            .GetByBranchAsync(branchId);

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("department/{departmentId}")]
    public async Task<IActionResult> GetByDepartment(
        int departmentId)
    {
        var data = await _service
            .GetByDepartmentAsync(departmentId);

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CostCenterDto dto)
    {
        try
        {
            var data = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = data.Id },
                new
                {
                    success = true,
                    message = "CostCenter created successfully.",
                    data
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CostCenterDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "CostCenter not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "CostCenter updated successfully."
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "CostCenter not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "CostCenter deleted successfully."
        });
    }
}