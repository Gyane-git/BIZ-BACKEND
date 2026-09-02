using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IAgentService
{
    Task<List<AgentDto>> GetAllAsync();

    Task<AgentDto?> GetByIdAsync(int id);

    Task<AgentDto> CreateAsync(AgentDto dto);

    Task<bool> UpdateAsync(int id, AgentDto dto);

    Task<bool> DeleteAsync(int id);
}