using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebitNoteController : ControllerBase
{
    private readonly IDebitNoteService _service;

    public DebitNoteController(IDebitNoteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("number/{debitNoteNumber}")]
    public async Task<IActionResult> GetByNumber(
        string debitNoteNumber)
    {
        var result =
            await _service.GetByNumberAsync(debitNoteNumber);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("fiscal-year/{fiscalYearId}")]
    public async Task<IActionResult> GetByFiscalYear(
        int fiscalYearId)
    {
        return Ok(
            await _service.GetByFiscalYearAsync(fiscalYearId));
    }

    [HttpGet("period/{fiscalYearPeriodId}")]
    public async Task<IActionResult> GetByPeriod(
        int fiscalYearPeriodId)
    {
        return Ok(
            await _service.GetByPeriodAsync(fiscalYearPeriodId));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        DebitNoteDto dto)
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
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        DebitNoteDto dto)
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
                    "Debit note updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(int id)
    {
        try
        {
            await _service.PostAsync(id);

            return Ok(new
            {
                message =
                    "Debit note posted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id}")]
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
                    "Debit note deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}