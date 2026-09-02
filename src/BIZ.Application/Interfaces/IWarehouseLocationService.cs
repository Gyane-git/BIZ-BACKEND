using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IWarehouseLocationService
{
    Task<List<WarehouseLocationDto>> GetAllAsync();

    Task<List<WarehouseLocationDto>> GetByWarehouseAsync(
        int warehouseId);

    Task<WarehouseLocationDto?> GetByIdAsync(int id);

    Task<WarehouseLocationDto> CreateAsync(
        WarehouseLocationDto dto);

    Task<bool> UpdateAsync(
        int id,
        WarehouseLocationDto dto);

    Task<bool> DeleteAsync(int id);
}