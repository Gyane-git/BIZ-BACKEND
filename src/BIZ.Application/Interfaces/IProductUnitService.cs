using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductUnitService
{
    Task<List<ProductUnitDto>> GetAllAsync();

    Task<List<ProductUnitDto>> GetByProductAsync(int productId);

    Task<ProductUnitDto?> GetByIdAsync(int id);

    Task<ProductUnitDto> CreateAsync(ProductUnitDto dto);

    Task<bool> UpdateAsync(int id, ProductUnitDto dto);

    Task<bool> DeleteAsync(int id);
}