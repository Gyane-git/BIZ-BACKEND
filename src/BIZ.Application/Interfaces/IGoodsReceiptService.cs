using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IGoodsReceiptService
{
    Task<IEnumerable<GoodsReceiptDto>> GetAllAsync();

    Task<GoodsReceiptDto?> GetByIdAsync(int id);

    Task<GoodsReceiptDto> CreateAsync(GoodsReceiptDto dto);

    Task<bool> UpdateAsync(int id, GoodsReceiptDto dto);

    Task<bool> DeleteAsync(int id);
}