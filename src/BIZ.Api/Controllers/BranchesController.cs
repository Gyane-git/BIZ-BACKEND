using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    // ============================================================
    // GET: api/Branches
    // ============================================================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var branches = await _branchService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = branches
        });
    }

    // ============================================================
    // GET: api/Branches/{id}
    // ============================================================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var branch = await _branchService.GetByIdAsync(id);

        if (branch is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Branch not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = branch
        });
    }

    // ============================================================
    // POST: api/Branches
    // ============================================================

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Branch branch)
    {
        var createdBranch = await _branchService.CreateAsync(branch);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdBranch.Id },
            new
            {
                success = true,
                message = "Branch created successfully.",
                data = createdBranch
            });
    }

    // ============================================================
    // PUT: api/Branches/{id}
    // ============================================================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] Branch branch)
    {
        var updated = await _branchService.UpdateAsync(id, branch);

        if (!updated)
        {
            return NotFound(new
            {
                success = false,
                message = "Branch not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Branch updated successfully."
        });
    }

    // ============================================================
    // DELETE: api/Branches/{id}
    // ============================================================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _branchService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Branch not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Branch deleted successfully."
        });
    }
}