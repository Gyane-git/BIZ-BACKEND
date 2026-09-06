using BIZ.Application.DTOs;

namespace BIZ.Application.Interfaces;

public interface IStockCountLineService
{
    Task<IEnumerable<StockCountLineDto>> GetAllAsync();

    Task<StockCountLineDto?> GetByIdAsync(int id);

    Task<IEnumerable<StockCountLineDto>> GetByCountAsync(
        int stockCountId);

    Task<StockCountLineDto> CreateAsync(
        StockCountLineDto dto);

    Task<bool> UpdateAsync(
        int id,
        StockCountLineDto dto);

    Task<bool> DeleteAsync(int id);
}