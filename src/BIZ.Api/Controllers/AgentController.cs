using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BIZ.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;

    public AgentController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _agentService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = agents
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var agent = await _agentService.GetByIdAsync(id);

        if (agent is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Agent with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = agent
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(AgentDto dto)
    {
        try
        {
            var agent = await _agentService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = agent.Id },
                new
                {
                    success = true,
                    message = "Agent created successfully.",
                    data = agent
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
        AgentDto dto)
    {
        try
        {
            var updated = await _agentService.UpdateAsync(id, dto);

            if (!updated)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Agent with id {id} was not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Agent updated successfully."
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
        var deleted = await _agentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                success = false,
                message = $"Agent with id {id} was not found."
            });
        }

        return Ok(new
        {
            success = true,
            message = "Agent deleted successfully."
        });
    }
}