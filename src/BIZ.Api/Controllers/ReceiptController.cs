using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptController : ControllerBase
{
    private readonly IReceiptService _service;

    public ReceiptController(
        IReceiptService service)
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
        var result =
            await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(
                "Receipt not found.");

        return Ok(result);
    }


    [HttpGet("number/{receiptNumber}")]
    public async Task<IActionResult> GetByNumber(
        string receiptNumber)
    {
        var result =
            await _service.GetByNumberAsync(
                receiptNumber);

        if (result == null)
            return NotFound(
                "Receipt not found.");

        return Ok(result);
    }


    [HttpGet("journal/{journalId:int}")]
    public async Task<IActionResult> GetByJournal(
        int journalId)
    {
        var result =
            await _service.GetByJournalAsync(
                journalId);

        return Ok(result);
    }


    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ReceiptDto dto)
    {
        try
        {
            var result =
                await _service.CreateAsync(dto);

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
        [FromBody] ReceiptDto dto)
    {
        try
        {
            var result =
                await _service.UpdateAsync(
                    id,
                    dto);

            if (!result)
                return NotFound(
                    "Receipt not found.");

            return Ok(new
            {
                message =
                    "Receipt updated successfully."
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
            var result =
                await _service.DeleteAsync(id);

            if (!result)
                return NotFound(
                    "Receipt not found.");

            return Ok(new
            {
                message =
                    "Receipt deactivated successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}