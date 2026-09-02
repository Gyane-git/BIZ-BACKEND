using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductImageController : ControllerBase
{
    private readonly IProductImageService _service;

    public ProductImageController(
        IProductImageService service)
    {
        _service = service;
    }

    // GET: api/ProductImage
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

    // GET: api/ProductImage/product/1
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

    // GET: api/ProductImage/1
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service
            .GetByIdAsync(id);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "ProductImage not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    // POST
    [HttpPost]
    public async Task<IActionResult> Create(
        ProductImageDto dto)
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
                    message = "ProductImage created successfully.",
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

    // PUT
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        ProductImageDto dto)
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
                    message = "ProductImage not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "ProductImage updated successfully."
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

    // DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);

        if (!result)
        {
            return NotFound(new
            {
                success = false,
                message = "ProductImage not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "ProductImage deleted successfully."
        });
    }
}