using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CashAccountController : ControllerBase
{
    private readonly ICashAccountService _service;

    public CashAccountController(
        ICashAccountService service)
    {
        _service = service;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }


    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var result = await _service
            .GetByIdAsync(id);

        if (result == null)
            return NotFound(
                "Cash Account not found.");

        return Ok(result);
    }


    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(
        string code)
    {
        var result = await _service
            .GetByCodeAsync(code);

        if (result == null)
            return NotFound(
                "Cash Account not found.");

        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CashAccountDto dto)
    {
        try
        {
            var result = await _service
                .CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CashAccountDto dto)
    {
        try
        {
            var result = await _service
                .UpdateAsync(id, dto);

            if (!result)
                return NotFound(
                    "Cash Account not found.");

            return Ok(new
            {
                message =
                    "Cash Account updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        try
        {
            var result = await _service
                .DeleteAsync(id);

            if (!result)
                return NotFound(
                    "Cash Account not found.");

            return Ok(new
            {
                message =
                    "Cash Account deactivated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}