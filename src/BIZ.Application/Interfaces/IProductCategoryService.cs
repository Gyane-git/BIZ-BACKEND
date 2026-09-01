using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategoryDto>> GetAllAsync();

    Task<ProductCategoryDto?> GetByIdAsync(int id);

    Task<ProductCategoryDto> CreateAsync(ProductCategoryDto dto);

    Task<bool> UpdateAsync(int id, ProductCategoryDto dto);

    Task<bool> DeleteAsync(int id);
}