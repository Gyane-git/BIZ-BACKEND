using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupplierController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SupplierController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var suppliers = await _supplierService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = suppliers
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var supplier = await _supplierService.GetByIdAsync(id);

        if (supplier is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Supplier with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = supplier
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(SupplierDto dto)
    {
        try
        {
            var supplier = await _supplierService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = supplier.Id },
                new
                {
                    success = true,
                    message = "Supplier created successfully.",
                    data = supplier
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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        SupplierDto dto)
    {
        try
        {
            var updated = await _supplierService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Supplier with id {id} was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Supplier updated successfully."
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

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _supplierService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = $"Supplier with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Supplier deleted successfully."
        });
    }
}