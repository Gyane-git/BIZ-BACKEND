using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockCountService
{
    Task<IEnumerable<StockCountDto>> GetAllAsync();

    Task<StockCountDto?> GetByIdAsync(int id);

    Task<StockCountDto> CreateAsync(
        StockCountDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockCountDto dto);

    Task<bool> DeleteAsync(int id);

    Task<bool> PostAsync(int id);
}