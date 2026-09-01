using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IUnitConversionService
{
    Task<IEnumerable<UnitConversionDto>> GetAllAsync();

    Task<UnitConversionDto?> GetByIdAsync(int id);

    Task<UnitConversionDto> CreateAsync(UnitConversionDto dto);

    Task<bool> UpdateAsync(int id, UnitConversionDto dto);

    Task<bool> DeleteAsync(int id);
}