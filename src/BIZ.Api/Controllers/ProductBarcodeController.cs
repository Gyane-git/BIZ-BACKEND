using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductBarcodeController : ControllerBase
{
    private readonly IProductBarcodeService _service;

    public ProductBarcodeController(
        IProductBarcodeService service)
    {
        _service = service;
    }

    // GET: api/ProductBarcode
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

    // GET: api/ProductBarcode/product/1
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

    // GET: api/ProductBarcode/barcode/123456
    [HttpGet("barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode(
        string barcode)
    {
        var data = await _service
            .GetByBarcodeAsync(barcode);

        if (data is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Barcode not found."
            });
        }

        return Ok(new
        {
            success = true,
            data
        });
    }

    // GET: api/ProductBarcode/1
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
                message = "ProductBarcode not found."
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
        ProductBarcodeDto dto)
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
                    message = "ProductBarcode created successfully.",
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
        ProductBarcodeDto dto)
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
                    message = "ProductBarcode not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "ProductBarcode updated successfully."
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
                message = "ProductBarcode not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "ProductBarcode deleted successfully."
        });
    }
}