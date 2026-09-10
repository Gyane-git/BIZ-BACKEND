using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductCompositionService
{
    Task<IEnumerable<ProductCompositionDto>> GetAllAsync();

    Task<ProductCompositionDto?> GetByIdAsync(int id);

    Task<ProductCompositionDto> CreateAsync(ProductCompositionDto dto);

    Task<bool> UpdateAsync(int id, ProductCompositionDto dto);

    Task<bool> DeleteAsync(int id);
}