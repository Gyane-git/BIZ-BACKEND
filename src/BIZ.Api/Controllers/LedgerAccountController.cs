using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LedgerAccountController : ControllerBase
{
    private readonly ILedgerAccountService _service;

    public LedgerAccountController(
        ILedgerAccountService service)
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

    [HttpGet("account-sub-group/{accountSubGroupId:int}")]
    public async Task<IActionResult> GetByAccountSubGroup(
        int accountSubGroupId)
    {
        var data =
            await _service.GetByAccountSubGroupAsync(
                accountSubGroupId);

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

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "LedgerAccount not found."
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
        var data =
            await _service.GetByCodeAsync(code);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "LedgerAccount not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] LedgerAccountDto dto)
    {
        try
        {
            var data =
                await _service.CreateAsync(dto);

            return Ok(new
            {
                success = true,
                message = "LedgerAccount created successfully.",
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
        [FromBody] LedgerAccountDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "LedgerAccount not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "LedgerAccount updated successfully."
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
        var result =
            await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "LedgerAccount not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "LedgerAccount deleted successfully."
        });
    }
}