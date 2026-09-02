using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductAttributeController : ControllerBase
{
    private readonly IProductAttributeService _service;

    public ProductAttributeController(
        IProductAttributeService service)
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

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(
        int productId)
    {
        var data = await _service
            .GetByProductAsync(productId);

        return Ok(new
        {
            success = true,
            data
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var data = await _service
            .GetByIdAsync(id);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "ProductAttribute not found."
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
        ProductAttributeDto dto)
    {
        try
        {
            var data = await _service
                .CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = data.Id },
                new
                {
                    success = true,
                    message = "ProductAttribute created successfully.",
                    data
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        ProductAttributeDto dto)
    {
        try
        {
            var result = await _service
                .UpdateAsync(id, dto);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "ProductAttribute not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "ProductAttribute updated successfully."
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var result = await _service
            .DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "ProductAttribute not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "ProductAttribute deleted successfully."
        });
    }
}