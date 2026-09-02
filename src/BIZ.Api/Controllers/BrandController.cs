using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    // GET: api/Brand
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var brands = await _brandService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = brands
        });
    }

    // GET: api/Brand/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var brand = await _brandService.GetByIdAsync(id);

        if (brand is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Brand with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = brand
        });
    }

    // POST: api/Brand
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BrandDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var brand = await _brandService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = brand.Id },
                new
                {
                    success = true,
                    message = "Brand created successfully.",
                    data = brand
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

    // PUT: api/Brand/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] BrandDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _brandService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Brand with id {id} was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Brand updated successfully."
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

    // DELETE: api/Brand/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _brandService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = $"Brand with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Brand deleted successfully."
        });
    }
}