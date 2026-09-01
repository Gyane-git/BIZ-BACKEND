using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IUnitService
{
    Task<IEnumerable<UnitDto>> GetAllAsync();

    Task<UnitDto?> GetByIdAsync(int id);

    Task<UnitDto> CreateAsync(UnitDto dto);

    Task<bool> UpdateAsync(int id, UnitDto dto);

    Task<bool> DeleteAsync(int id);
}