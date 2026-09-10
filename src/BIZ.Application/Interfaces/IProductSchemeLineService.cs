using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductSchemeLineService
{
    Task<IEnumerable<ProductSchemeLineDto>> GetAllAsync();

    Task<ProductSchemeLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<ProductSchemeLineDto>> GetBySchemeAsync(int productSchemeId);

    Task<ProductSchemeLineDto> CreateAsync(ProductSchemeLineDto dto);

    Task<bool> UpdateAsync(int id, ProductSchemeLineDto dto);

    Task<bool> DeleteAsync(int id);
}