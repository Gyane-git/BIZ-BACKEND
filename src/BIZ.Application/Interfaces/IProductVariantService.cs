using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductVariantService
{
    Task<List<ProductVariantDto>> GetAllAsync();

    Task<List<ProductVariantDto>> GetByProductAsync(
        int productId);

    Task<ProductVariantDto?> GetByIdAsync(
        int id);

    Task<ProductVariantDto?> GetByCodeAsync(
        string variantCode);

    Task<ProductVariantDto> CreateAsync(
        ProductVariantDto dto);

    Task<bool> UpdateAsync(
        int id,
        ProductVariantDto dto);

    Task<bool> DeleteAsync(
        int id);
}