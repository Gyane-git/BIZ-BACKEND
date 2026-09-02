using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductVariantController : ControllerBase
{
    private readonly IProductVariantService _service;

    public ProductVariantController(
        IProductVariantService service)
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

    [HttpGet("code/{variantCode}")]
    public async Task<IActionResult> GetByCode(
        string variantCode)
    {
        var data = await _service
            .GetByCodeAsync(variantCode);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "ProductVariant not found."
            });
        }

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
                message = "ProductVariant not found."
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
        ProductVariantDto dto)
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
                    message = "ProductVariant created successfully.",
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
        ProductVariantDto dto)
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
                    message = "ProductVariant not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "ProductVariant updated successfully."
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
                message = "ProductVariant not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "ProductVariant deleted successfully."
        });
    }
}