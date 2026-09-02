using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductImageService
{
    Task<List<ProductImageDto>> GetAllAsync();

    Task<List<ProductImageDto>> GetByProductAsync(int productId);

    Task<ProductImageDto?> GetByIdAsync(int id);

    Task<ProductImageDto> CreateAsync(ProductImageDto dto);

    Task<bool> UpdateAsync(int id, ProductImageDto dto);

    Task<bool> DeleteAsync(int id);
}
