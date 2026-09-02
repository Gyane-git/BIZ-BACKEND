using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IProductBarcodeService
{
    Task<List<ProductBarcodeDto>> GetAllAsync();

    Task<List<ProductBarcodeDto>> GetByProductAsync(int productId);

    Task<ProductBarcodeDto?> GetByIdAsync(int id);

    Task<ProductBarcodeDto?> GetByBarcodeAsync(string barcode);

    Task<ProductBarcodeDto> CreateAsync(ProductBarcodeDto dto);

    Task<bool> UpdateAsync(int id, ProductBarcodeDto dto);

    Task<bool> DeleteAsync(int id);
}