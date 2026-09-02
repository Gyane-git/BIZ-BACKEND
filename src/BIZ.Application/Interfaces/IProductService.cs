using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto?> GetByCodeAsync(string code);

    Task<ProductDto> CreateAsync(ProductDto dto);

    Task<bool> UpdateAsync(int id, ProductDto dto);

    Task<bool> DeleteAsync(int id);
}