using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductSubGroupService
{
    Task<IEnumerable<ProductSubGroupDto>> GetAllAsync();

    Task<IEnumerable<ProductSubGroupDto>> GetByGroupIdAsync(int productGroupId);

    Task<ProductSubGroupDto?> GetByIdAsync(int id);

    Task<ProductSubGroupDto> CreateAsync(ProductSubGroupDto dto);

    Task<bool> UpdateAsync(int id, ProductSubGroupDto dto);

    Task<bool> DeleteAsync(int id);
}