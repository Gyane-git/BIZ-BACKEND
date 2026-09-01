using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryService _service;

    public ProductCategoryController(
        IProductCategoryService service)
    {
        _service = service;
    }

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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Product category not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        ProductCategoryDto dto)
    {
        try
        {
            var data = await _service.CreateAsync(dto);

            return Ok(new
            {
                success = true,
                message = "Product category created successfully.",
                data
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        ProductCategoryDto dto)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product category not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Product category updated successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Product category not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Product category deleted successfully."
        });
    }
}