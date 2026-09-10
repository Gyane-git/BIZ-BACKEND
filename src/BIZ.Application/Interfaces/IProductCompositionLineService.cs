using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductCompositionLineService
{
    Task<IEnumerable<ProductCompositionLineDto>> GetAllAsync();

    Task<ProductCompositionLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<ProductCompositionLineDto>> GetByCompositionAsync(
        int productCompositionId);

    Task<ProductCompositionLineDto> CreateAsync(ProductCompositionLineDto dto);

    Task<bool> UpdateAsync(int id, ProductCompositionLineDto dto);

    Task<bool> DeleteAsync(int id);
}