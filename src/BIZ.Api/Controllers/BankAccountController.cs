using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BankAccountController : ControllerBase
{
    private readonly IBankAccountService _service;

    public BankAccountController(
        IBankAccountService service)
    {
        _service = service;
    }


    // =========================================================
    // GET ALL
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }


    // =========================================================
    // GET BY ID
    // =========================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var result = await _service
            .GetByIdAsync(id);

        if (result == null)
            return NotFound(
                "Bank Account not found.");

        return Ok(result);
    }


    // =========================================================
    // GET BY ACCOUNT NUMBER
    // =========================================================

    [HttpGet("account-number/{accountNumber}")]
    public async Task<IActionResult> GetByAccountNumber(
        string accountNumber)
    {
        var result = await _service
            .GetByAccountNumberAsync(accountNumber);

        if (result == null)
            return NotFound(
                "Bank Account not found.");

        return Ok(result);
    }


    // =========================================================
    // CREATE
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] BankAccountDto dto)
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


    // =========================================================
    // UPDATE
    // =========================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BankAccountDto dto)
    {
        try
        {
            var result = await _service
                .UpdateAsync(id, dto);

            if (!result)
                return NotFound(
                    "Bank Account not found.");

            return Ok(new
            {
                message =
                    "Bank Account updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }


    // =========================================================
    // DELETE
    // =========================================================

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
                    "Bank Account not found.");

            return Ok(new
            {
                message =
                    "Bank Account deactivated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}