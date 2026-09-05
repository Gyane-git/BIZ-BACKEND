using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IGoodsReceiptLineService
{
    Task<IEnumerable<GoodsReceiptLineDto>> GetAllAsync();

    Task<GoodsReceiptLineDto?> GetByIdAsync(int id);

    Task<GoodsReceiptLineDto> CreateAsync(GoodsReceiptLineDto dto);

    Task<bool> UpdateAsync(int id, GoodsReceiptLineDto dto);

    Task<bool> DeleteAsync(int id);
}