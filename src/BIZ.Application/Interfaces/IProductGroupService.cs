using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductGroupService
{
    Task<IEnumerable<ProductGroupDto>> GetAllAsync();

    Task<ProductGroupDto?> GetByIdAsync(int id);

    Task<ProductGroupDto> CreateAsync(ProductGroupDto dto);

    Task<bool> UpdateAsync(int id, ProductGroupDto dto);

    Task<bool> DeleteAsync(int id);
}