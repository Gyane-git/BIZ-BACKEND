using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyUnitsController : ControllerBase
{
    private readonly ICompanyUnitService _companyUnitService;

    public CompanyUnitsController(
        ICompanyUnitService companyUnitService)
    {
        _companyUnitService = companyUnitService;
    }

    // ============================================================
    // GET: api/CompanyUnits
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var companyUnits =
            await _companyUnitService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = companyUnits
        });
    }

    // ============================================================
    // GET: api/CompanyUnits/{id}
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var companyUnit =
            await _companyUnitService.GetByIdAsync(id);

        if (companyUnit is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Company unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = companyUnit
        });
    }

    // ============================================================
    // POST: api/CompanyUnits
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CompanyUnit companyUnit)
    {
        var created =
            await _companyUnitService.CreateAsync(companyUnit);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new
            {
                success = true,
                message = "Company unit created successfully.",
                data = created
            });
    }

    // ============================================================
    // PUT: api/CompanyUnits/{id}
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CompanyUnit companyUnit)
    {
        var updated =
            await _companyUnitService.UpdateAsync(
                id,
                companyUnit);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message = "Company unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Company unit updated successfully."
        });
    }

    // ============================================================
    // DELETE: api/CompanyUnits/{id}
    // ============================================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _companyUnitService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Company unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Company unit deleted successfully."
        });
    }
}