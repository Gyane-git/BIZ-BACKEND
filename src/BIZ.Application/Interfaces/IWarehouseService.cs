using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IWarehouseService
{
    Task<List<WarehouseDto>> GetAllAsync();

    Task<WarehouseDto?> GetByIdAsync(int id);

    Task<WarehouseDto> CreateAsync(WarehouseDto dto);

    Task<bool> UpdateAsync(int id, WarehouseDto dto);

    Task<bool> DeleteAsync(int id);
}