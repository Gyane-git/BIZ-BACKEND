using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductSchemeService
{
    Task<IEnumerable<ProductSchemeDto>> GetAllAsync();

    Task<ProductSchemeDto?> GetByIdAsync(int id);

    Task<ProductSchemeDto> CreateAsync(ProductSchemeDto dto);

    Task<bool> UpdateAsync(int id, ProductSchemeDto dto);

    Task<bool> DeleteAsync(int id);
}