using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductBatchService
{
    Task<List<ProductBatchDto>> GetAllAsync();

    Task<List<ProductBatchDto>> GetByProductAsync(
        int productId);

    Task<List<ProductBatchDto>> GetByVariantAsync(
        int productVariantId);

    Task<ProductBatchDto?> GetByIdAsync(
        int id);

    Task<ProductBatchDto?> GetByBatchNumberAsync(
        string batchNumber);

    Task<ProductBatchDto> CreateAsync(
        ProductBatchDto dto);

    Task<bool> UpdateAsync(
        int id,
        ProductBatchDto dto);

    Task<bool> DeleteAsync(
        int id);
}