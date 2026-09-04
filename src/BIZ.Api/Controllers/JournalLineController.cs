using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalLineController : ControllerBase
{
    private readonly IJournalLineService _service;

    public JournalLineController(
        IJournalLineService service)
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
    // GET BY JOURNAL
    // =========================================================

    [HttpGet("journal/{journalId:int}")]
    public async Task<IActionResult> GetByJournal(
        int journalId)
    {
        var result = await _service
            .GetByJournalAsync(journalId);

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
                "Journal Line not found.");

        return Ok(result);
    }


    // =========================================================
    // CREATE
    // =========================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] JournalLineDto dto)
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
        [FromBody] JournalLineDto dto)
    {
        try
        {
            var result = await _service
                .UpdateAsync(id, dto);

            if (!result)
                return NotFound(
                    "Journal Line not found.");

            return Ok(new
            {
                message =
                    "Journal Line updated successfully."
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
                    "Journal Line not found.");

            return Ok(new
            {
                message =
                    "Journal Line deleted successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}