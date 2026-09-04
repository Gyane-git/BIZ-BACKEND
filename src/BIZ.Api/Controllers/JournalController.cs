using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalController : ControllerBase
{
    private readonly IJournalService _service;

    public JournalController(IJournalService service)
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
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound("Journal not found.");

        return Ok(result);
    }

    [HttpGet("number/{journalNumber}")]
    public async Task<IActionResult> GetByNumber(
        string journalNumber)
    {
        var result =
            await _service.GetByNumberAsync(journalNumber);

        if (result == null)
            return NotFound("Journal not found.");

        return Ok(result);
    }

    [HttpGet("fiscal-year/{fiscalYearId:int}")]
    public async Task<IActionResult> GetByFiscalYear(
        int fiscalYearId)
    {
        return Ok(
            await _service.GetByFiscalYearAsync(
                fiscalYearId));
    }

    [HttpGet("period/{fiscalYearPeriodId:int}")]
    public async Task<IActionResult> GetByPeriod(
        int fiscalYearPeriodId)
    {
        return Ok(
            await _service.GetByPeriodAsync(
                fiscalYearPeriodId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] JournalDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);

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
        [FromBody] JournalDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound("Journal not found.");

            return Ok(new
            {
                message =
                    "Journal updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        try
        {
            var result = await _service.PostAsync(id);

            if (!result)
                return NotFound("Journal not found.");

            return Ok(new
            {
                message =
                    "Journal posted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound("Journal not found.");

            return Ok(new
            {
                message =
                    "Journal deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}