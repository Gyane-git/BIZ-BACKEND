using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiscalYearPeriodController : ControllerBase
{
    private readonly IFiscalYearPeriodService _service;

    public FiscalYearPeriodController(
        IFiscalYearPeriodService service)
    {
        _service = service;
    }

    // GET: api/FiscalYearPeriod
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }

    // GET: api/FiscalYearPeriod/fiscal-year/1
    [HttpGet("fiscal-year/{fiscalYearId:int}")]
    public async Task<IActionResult> GetByFiscalYear(
        int fiscalYearId)
    {
        var result = await _service
            .GetByFiscalYearAsync(fiscalYearId);

        return Ok(result);
    }

    // GET: api/FiscalYearPeriod/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service
            .GetByIdAsync(id);

        if (result == null)
            return NotFound(
                "Fiscal Year Period not found.");

        return Ok(result);
    }

    // GET: api/FiscalYearPeriod/code/FY2026-P01
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(
        string code)
    {
        var result = await _service
            .GetByCodeAsync(code);

        if (result == null)
            return NotFound(
                "Fiscal Year Period not found.");

        return Ok(result);
    }

    // GET: api/FiscalYearPeriod/current/1
    [HttpGet("current/{fiscalYearId:int}")]
    public async Task<IActionResult> GetCurrent(
        int fiscalYearId)
    {
        var result = await _service
            .GetCurrentAsync(fiscalYearId);

        if (result == null)
            return NotFound(
                "Current Fiscal Year Period not found.");

        return Ok(result);
    }

    // POST: api/FiscalYearPeriod
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] FiscalYearPeriodDto dto)
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

    // PUT: api/FiscalYearPeriod/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] FiscalYearPeriodDto dto)
    {
        try
        {
            var result = await _service
                .UpdateAsync(id, dto);

            if (!result)
                return NotFound(
                    "Fiscal Year Period not found.");

            return Ok(new
            {
                message =
                    "Fiscal Year Period updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST: api/FiscalYearPeriod/1/close
    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id)
    {
        try
        {
            var result = await _service
                .CloseAsync(id);

            if (!result)
                return NotFound(
                    "Fiscal Year Period not found.");

            return Ok(new
            {
                message =
                    "Fiscal Year Period closed successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/FiscalYearPeriod/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service
                .DeleteAsync(id);

            if (!result)
                return NotFound(
                    "Fiscal Year Period not found.");

            return Ok(new
            {
                message =
                    "Fiscal Year Period deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}