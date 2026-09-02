using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModelController : ControllerBase
{
    private readonly IModelService _modelService;

    public ModelController(IModelService modelService)
    {
        _modelService = modelService;
    }

    // GET: api/Model
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var models = await _modelService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = models
        });
    }

    // GET: api/Model/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var model = await _modelService.GetByIdAsync(id);

        if (model is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Model with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = model
        });
    }

    // POST: api/Model
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] ModelDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var model = await _modelService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = model.Id },
                new
                {
                    success = true,
                    message = "Model created successfully.",
                    data = model
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

    // PUT: api/Model/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ModelDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var updated = await _modelService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Model with id {id} was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Model updated successfully."
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

    // DELETE: api/Model/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _modelService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = $"Model with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Model deleted successfully."
        });
    }
}