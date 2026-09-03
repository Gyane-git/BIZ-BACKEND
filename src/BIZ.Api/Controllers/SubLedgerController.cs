using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubLedgerController : ControllerBase
{
    private readonly ISubLedgerService _service;

    public SubLedgerController(ISubLedgerService service)
    {
        _service = service;
    }

    // GET: api/SubLedger
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

    // GET: api/SubLedger/ledger-account/1
    [HttpGet("ledger-account/{ledgerAccountId}")]
    public async Task<IActionResult> GetByLedgerAccount(
        int ledgerAccountId)
    {
        var data = await _service
            .GetByLedgerAccountAsync(ledgerAccountId);

        return Ok(new
        {
            success = true,
            data
        });
    }

    // GET: api/SubLedger/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data == null)
        {
            return NotFound(new
            {
                success = false,
                message = "SubLedger not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    // GET: api/SubLedger/code/CUST001
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var data = await _service.GetByCodeAsync(code);

        if (data == null)
        {
            return NotFound(new
            {
                success = false,
                message = "SubLedger not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    // POST: api/SubLedger
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] SubLedgerDto dto)
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
                    message = "SubLedger created successfully.",
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

    // PUT: api/SubLedger/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] SubLedgerDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "SubLedger not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "SubLedger updated successfully."
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

    // DELETE: api/SubLedger/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "SubLedger not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "SubLedger deleted successfully."
        });
    }
}