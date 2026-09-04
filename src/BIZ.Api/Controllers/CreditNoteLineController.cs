using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditNoteLineController : ControllerBase
{
    private readonly ICreditNoteLineService _service;

    public CreditNoteLineController(ICreditNoteLineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("credit-note/{creditNoteId}")]
    public async Task<IActionResult> GetByCreditNote(int creditNoteId)
    {
        return Ok(await _service.GetByCreditNoteAsync(creditNoteId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreditNoteLineDto dto)
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
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        CreditNoteLineDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message = "Credit note line updated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id);

            if (!result)
                return NotFound();

            return Ok(new
            {
                message = "Credit note line deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}