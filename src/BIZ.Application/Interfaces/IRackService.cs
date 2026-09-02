using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IRackService
{
    Task<List<RackDto>> GetAllAsync();

    Task<RackDto?> GetByIdAsync(int id);

    Task<RackDto> CreateAsync(RackDto dto);

    Task<bool> UpdateAsync(int id, RackDto dto);

    Task<bool> DeleteAsync(int id);
}