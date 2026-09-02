using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AccountSubGroupController : ControllerBase
{
    private readonly IAccountSubGroupService _service;

    public AccountSubGroupController(
        IAccountSubGroupService service)
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

    [HttpGet("account-group/{accountGroupId:int}")]
    public async Task<IActionResult> GetByAccountGroup(
        int accountGroupId)
    {
        var data = await _service
            .GetByAccountGroupAsync(accountGroupId);

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
                message = "AccountSubGroup not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var data = await _service.GetByCodeAsync(code);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "AccountSubGroup not found."
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
        [FromBody] AccountSubGroupDto dto)
    {
        try
        {
            var data = await _service.CreateAsync(dto);

            return Ok(new
            {
                success = true,
                message = "AccountSubGroup created successfully.",
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
        [FromBody] AccountSubGroupDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "AccountSubGroup not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "AccountSubGroup updated successfully."
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
                message = "AccountSubGroup not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "AccountSubGroup deleted successfully."
        });
    }
}