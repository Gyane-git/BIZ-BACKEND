using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductSerialService
{
    Task<List<ProductSerialDto>> GetAllAsync();

    Task<List<ProductSerialDto>> GetByProductAsync(
        int productId);

    Task<List<ProductSerialDto>> GetByBatchAsync(
        int productBatchId);

    Task<ProductSerialDto?> GetByIdAsync(
        int id);

    Task<ProductSerialDto?> GetBySerialNumberAsync(
        string serialNumber);

    Task<ProductSerialDto> CreateAsync(
        ProductSerialDto dto);

    Task<bool> UpdateAsync(
        int id,
        ProductSerialDto dto);

    Task<bool> DeleteAsync(
        int id);
}