using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductAttributeService
{
    Task<List<ProductAttributeDto>> GetAllAsync();

    Task<List<ProductAttributeDto>> GetByProductAsync(
        int productId);

    Task<ProductAttributeDto?> GetByIdAsync(
        int id);

    Task<ProductAttributeDto> CreateAsync(
        ProductAttributeDto dto);

    Task<bool> UpdateAsync(
        int id,
        ProductAttributeDto dto);

    Task<bool> DeleteAsync(
        int id);
}