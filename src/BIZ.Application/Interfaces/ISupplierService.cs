using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetAllAsync();

    Task<SupplierDto?> GetByIdAsync(int id);

    Task<SupplierDto> CreateAsync(SupplierDto dto);

    Task<bool> UpdateAsync(int id, SupplierDto dto);

    Task<bool> DeleteAsync(int id);
}