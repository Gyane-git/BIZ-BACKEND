using BIZ.Application.DTOs;
using BIZ.Application.Interfaces;
using BIZ.Domain.Entities;
using BIZ.Infrastructure.Persistence.Tenant;
using Microsoft.EntityFrameworkCore;

namespace BIZ.Infrastructure.Services;

public class AgentService : IAgentService
{
    private readonly TenantDbContext _db;

    public AgentService(TenantDbContext db)
    {
        _db = db;
    }

    public async Task<List<AgentDto>> GetAllAsync()
    {
        return await _db.Agents
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new AgentDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Email = x.Email,
                PanNumber = x.PanNumber,
                ContactPerson = x.ContactPerson,
                CommissionRate = x.CommissionRate,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<AgentDto?> GetByIdAsync(int id)
    {
        return await _db.Agents
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AgentDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                Phone = x.Phone,
                Email = x.Email,
                PanNumber = x.PanNumber,
                ContactPerson = x.ContactPerson,
                CommissionRate = x.CommissionRate,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AgentDto> CreateAsync(AgentDto dto)
    {
        var code = dto.Code.Trim();

        if (await _db.Agents.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException(
                $"Agent code '{code}' already exists.");
        }

        var agent = new Agent
        {
            Code = code,
            Name = dto.Name.Trim(),
            Address = dto.Address?.Trim(),
            Phone = dto.Phone?.Trim(),
            Email = dto.Email?.Trim(),
            PanNumber = dto.PanNumber?.Trim(),
            ContactPerson = dto.ContactPerson?.Trim(),
            CommissionRate = dto.CommissionRate,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Agents.Add(agent);

        await _db.SaveChangesAsync();

        dto.Id = agent.Id;

        return dto;
    }

    public async Task<bool> UpdateAsync(int id, AgentDto dto)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(x => x.Id == id);

        if (agent is null)
            return false;

        var code = dto.Code.Trim();

        if (await _db.Agents.AnyAsync(
            x => x.Code == code && x.Id != id))
        {
            throw new InvalidOperationException(
                $"Agent code '{code}' already exists.");
        }

        agent.Code = code;
        agent.Name = dto.Name.Trim();
        agent.Address = dto.Address?.Trim();
        agent.Phone = dto.Phone?.Trim();
        agent.Email = dto.Email?.Trim();
        agent.PanNumber = dto.PanNumber?.Trim();
        agent.ContactPerson = dto.ContactPerson?.Trim();
        agent.CommissionRate = dto.CommissionRate;
        agent.IsActive = dto.IsActive;
        agent.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var agent = await _db.Agents
            .FirstOrDefaultAsync(x => x.Id == id);

        if (agent is null)
            return false;

        _db.Agents.Remove(agent);

        await _db.SaveChangesAsync();

        return true;
    }
}